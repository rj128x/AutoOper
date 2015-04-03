using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO.Ports;
using System.Net.NetworkInformation;

namespace Modbus_TCP_Server {
	using System;
	using System.Net;
	using System.Net.NetworkInformation;
	using System.Net.Sockets;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using System.Threading;

	public class Slave {
		// Fields
		private static bool _connected = false;
		public static ushort _refresh = 10;
		public static ushort _timeout = 500;
		public bool AnswerRDY;
		public const byte excAck = 5;
		public const byte excExceptionConnectionLost = 254;
		public const byte excExceptionNotConnected = 253;
		public const byte excExceptionOffset = 128;
		public const byte excExceptionTimeout = 255;
		public const byte excGatePathUnavailable = 10;
		public const byte excIllegalDataAdr = 2;
		public const byte excIllegalDataVal = 3;
		public const byte excIllegalFunction = 1;
		public const byte excSendFailt = 100;
		public const byte excSlaveDeviceFailure = 4;
		public const byte excSlaveIsBusy = 6;
		public const byte fctReadCoils = 1;
		public const byte fctReadDiscreteInputs = 2;
		public const byte fctReadHoldingRegister = 3;
		public const byte fctReadInputRegister = 4;
		public const byte fctReadWriteMultipleRegister = 23;
		public const byte fctWriteMultipleCoils = 15;
		public const byte fctWriteMultipleRegister = 16;
		public const byte fctWriteSingleCoil = 5;
		public const byte fctWriteSingleRegister = 6;
		public byte[] InBuffer = new byte[64];
		public string IP;
		public TcpListener Listener;
		public bool[] ModbusCoils = new bool[100];
		public ModbusRegister[] ModbusRegisters = new ModbusRegister[100];
		public TcpClient MyTcpClient;
		public NetworkStream networkStream;
		private ushort NumberBytes;
		private const int NumberMBRegisters = 100;
		private ushort NumberRegs;
		public byte[] OutBuffer;
		public int PORT;
		public ushort Slave_ID;
		private ushort StartAddress;
		public Socket tcpSocket;

		// Methods
		private bool CheckAvailableServerPort(int port) {
			foreach (IPEndPoint point in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()) {
				if (point.Port == port) {
					return false;
				}
			}
			return true;
		}

		public void connect() {
			Thread.BeginCriticalRegion();
			this.MakeConnection();
			Thread.EndCriticalRegion();
		}

		public byte[] CreateAnswer_1(ushort id) {
			int num = ((this.NumberRegs % 8) == 0) ? (this.NumberRegs / 8) : ((this.NumberRegs / 8) + 1);
			byte[] buffer = new byte[num + 9];
			byte[] buffer2 = new byte[num];
			int startAddress = this.StartAddress;
			for (int i = 0; startAddress < this.NumberRegs; i++) {
				if (this.ModbusCoils[startAddress]) {
					buffer2[i / 8] = (byte)(((int)1) << (i % 8));
				}
				startAddress++;
			}
			buffer[0] = this.InBuffer[0];
			buffer[1] = this.InBuffer[1];
			buffer[2] = this.InBuffer[2];
			buffer[3] = this.InBuffer[3];
			buffer[4] = (byte)((3 + num) / 255);
			buffer[5] = (byte)((3 + num) % 255);
			buffer[6] = (byte)this.Slave_ID;
			buffer[7] = 1;
			buffer[8] = (byte)num;
			for (ushort j = 0; j < num; j = (ushort)(j + 1)) {
				buffer[9 + j] = buffer2[(num - j) - 1];
			}
			return buffer;
		}

		public byte[] CreateAnswer_15(ushort id) {
			return new byte[] { this.InBuffer[0], this.InBuffer[1], this.InBuffer[2], this.InBuffer[3], 0, 6, ((byte)this.Slave_ID), 15, this.InBuffer[8], this.InBuffer[9], this.InBuffer[10], this.InBuffer[11] };
		}

		public byte[] CreateAnswer_16(ushort id) {
			return new byte[] { this.InBuffer[0], this.InBuffer[1], this.InBuffer[2], this.InBuffer[3], 0, 6, ((byte)this.Slave_ID), 16, this.InBuffer[8], this.InBuffer[9], this.InBuffer[10], this.InBuffer[11] };
		}

		public byte[] CreateAnswer_3(ushort id) {
			byte[] buffer = new byte[(this.NumberRegs * 2) + 9];
			buffer[0] = this.InBuffer[0];
			buffer[1] = this.InBuffer[1];
			buffer[2] = this.InBuffer[2];
			buffer[3] = this.InBuffer[3];
			buffer[4] = (byte)((3 + (this.NumberRegs * 2)) / 255);
			buffer[5] = (byte)((3 + (this.NumberRegs * 2)) % 255);
			buffer[6] = (byte)this.Slave_ID;
			buffer[7] = 3;
			buffer[8] = (byte)(2 * this.NumberRegs);
			for (int i = 0; i < this.NumberRegs; i++) {
				buffer[9 + (i * 2)] = this.ModbusRegisters[this.StartAddress + i].HiByte;
				buffer[10 + (i * 2)] = this.ModbusRegisters[this.StartAddress + i].LoByte;
			}
			return buffer;
		}

		public void DiscardInBuffer() {
		}

		public void DiscardOutBuffer() {
		}

		public void disconnect() {
			this.Dispose();
		}

		public void Dispose() {
			_connected = false;
			if (this.Listener != null) {
				this.Listener.Stop();
				this.Listener = null;
			}
			if (this.MyTcpClient != null) {
				this.MyTcpClient = null;
			}
			if (this.networkStream != null) {
				this.networkStream.Close();
			}
		}

		~Slave() {
			this.Dispose();
		}

		public int GetNumberMBRegisters() {
			return 100;
		}

		private void MakeConnection() {
			lock (this) {
				if (this.CheckAvailableServerPort(this.PORT)) {
					this.Listener = new TcpListener(this.PORT);
					try {
						this.Listener.Start();
						this.MyTcpClient = this.Listener.AcceptTcpClient();
						this.networkStream = this.MyTcpClient.GetStream();
						this.networkStream.ReadTimeout = 7000;
					}
					catch (Exception) {
						_connected = false;
					}
					Thread.Sleep(30);
					if ((this.MyTcpClient != null) && this.MyTcpClient.Connected) {
						int num = 0;
						_connected = true;
						while (_connected) {
							for (int i = 0; i < this.InBuffer.Length; i++) {
								this.InBuffer[i] = 0;
							}
							try {
								num = this.networkStream.Read(this.InBuffer, 0, this.InBuffer.Length);
							}
							catch {
								Thread.Sleep(30);
								_connected = false;
							}
							try {
								if (num > 0) {
									ushort num3 = this.InBuffer[6];
									short num4 = Convert.ToInt16(this.InBuffer[7]);
									if (num3 == this.Slave_ID) {
										lock (this) {
											int num5;
											byte[] buffer9;
											int num6;
											switch (num4) {
												case 1: {
														byte[] buffer3 = new byte[] { this.InBuffer[9], this.InBuffer[8] };
														this.StartAddress = BitConverter.ToUInt16(buffer3, 0);
														byte[] buffer4 = new byte[] { this.InBuffer[11], this.InBuffer[10] };
														this.NumberRegs = BitConverter.ToUInt16(buffer4, 0);
														this.OutBuffer = this.CreateAnswer_1(this.Slave_ID);
														goto Label_042F;
													}
												case 3: {
														byte[] buffer = new byte[] { this.InBuffer[9], this.InBuffer[8] };
														this.StartAddress = BitConverter.ToUInt16(buffer, 0);
														byte[] buffer2 = new byte[] { this.InBuffer[11], this.InBuffer[10] };
														this.NumberRegs = BitConverter.ToUInt16(buffer2, 0);
														this.OutBuffer = this.CreateAnswer_3(this.Slave_ID);
														goto Label_042F;
													}
												case 15: {
														byte[] buffer7 = new byte[] { this.InBuffer[9], this.InBuffer[8] };
														this.StartAddress = BitConverter.ToUInt16(buffer7, 0);
														byte[] buffer8 = new byte[] { this.InBuffer[11], this.InBuffer[10] };
														this.NumberRegs = BitConverter.ToUInt16(buffer8, 0);
														this.NumberBytes = Convert.ToUInt16(this.InBuffer[12]);
														buffer9 = new byte[this.NumberBytes];
														num6 = 0;
														goto Label_03CB;
													}
												case 16: {
														byte[] buffer5 = new byte[] { this.InBuffer[9], this.InBuffer[8] };
														this.StartAddress = BitConverter.ToUInt16(buffer5, 0);
														byte[] buffer6 = new byte[] { this.InBuffer[11], this.InBuffer[10] };
														this.NumberRegs = BitConverter.ToUInt16(buffer6, 0);
														this.NumberBytes = Convert.ToUInt16(this.InBuffer[12]);
														num5 = 0;
														goto Label_0305;
													}
												default:
													goto Label_042F;
											}
										Label_02B1:
											this.ModbusRegisters[this.StartAddress + num5].HiByte = this.InBuffer[13 + (num5 * 2)];
											this.ModbusRegisters[this.StartAddress + num5].LoByte = this.InBuffer[14 + (num5 * 2)];
											num5++;
										Label_0305:
											if (num5 < this.NumberRegs) {
												goto Label_02B1;
											}
											this.OutBuffer = this.CreateAnswer_16(this.Slave_ID);
											goto Label_042F;
										Label_03AB:
											buffer9[(this.NumberBytes - num6) - 1] = this.InBuffer[13 + num6];
											num6++;
										Label_03CB:
											if (num6 < this.NumberBytes) {
												goto Label_03AB;
											}
											for (int j = 0; j < this.NumberRegs; j++) {
												if (Convert.ToBoolean((int)(buffer9[j / 8] & ((byte)(((int)1) << (j % 8)))))) {
													this.ModbusCoils[j] = false;
												}
												else {
													this.ModbusCoils[j] = true;
												}
											}
											this.OutBuffer = this.CreateAnswer_15(this.Slave_ID);
										Label_042F:
											this.networkStream.Write(this.OutBuffer, 0, this.OutBuffer.Length);
										}
									}
								}
								continue;
							}
							catch {
								this.disconnect();
								continue;
							}
						}
					}
				}
			}
		}

		// Properties
		public bool connected {
			get {
				return _connected;
			}
		}

		// Nested Types
		public delegate void ExceptionData(ushort id, byte function, byte exception);

		[StructLayout(LayoutKind.Sequential)]
		public struct ModbusRegister {
			public byte LoByte;
			public byte HiByte;
		}

		public delegate void ResponseData(ushort id, byte function, byte[] data);
	}

}
