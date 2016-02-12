using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using ZedGraph;

namespace Modbus_TCP_Server {
    public partial class Form1 : Form {
        private Slave MySlave;

        private Thread ServThread;          // поток работы модбас-сервера
        private int TimeFromMoskow;         // разница во времени с Москвой
        private int MoskowCurrHour = -1;    // текущий час в Москве
        public bool isMoskowDateChanged = false; // флаг изменения текущей даты в Москве
        public bool isAdminRules = false;   // права админа вкл/откл
        public bool isReadPGTPfromDB = false; // прочитан график измеренной мощности из  БД
        public double PGESyesterdayFACT = 0;     // сумма фактической выработки ГЭС за вчерашний день
        public double PGEStodayFACT = 0;               // сумма фактической выработки ГЭС за сегодня
        public double PGESyesterdayPLAN = 0;     // сумма плановой выработки ГЭС за вчерашний день
        public double PGEStodayPLAN = 0;               // сумма плановой выработки ГЭС за сегодня

        private double[] GTP0;
        private double[] GTP1;
        private double[] GTP2;
        private double[] GTP3;
        private double[] GTP4;
        private double[] GTP5;
        private double[] GTP6;
        private double[] GTP7;
        private double[] GTP8;
        private double[] GTP9;
        private double[] GTP10;

        private double[] GTP0_final;
        private double[] GTP1_final;
        private double[] GTP2_final;
        private double[] GTP3_final;
        private double[] GTP4_final;
        private double[] GTP5_final;
        private double[] GTP6_final;
        private double[] GTP7_final;
        private double[] GTP8_final;
        private double[] GTP9_final;
        private double[] GTP10_final;

        // словарь Время = мощность
        private Dictionary<int, double> GTP1_DB;
        private Dictionary<int, double> GTP2_DB;
        private Dictionary<int, double> GTP3_DB;
        private Dictionary<int, double> GTP4_DB;
        private Dictionary<int, double> GTP5_DB;
        private Dictionary<int, double> GTP6_DB;
        private Dictionary<int, double> GTP7_DB;
        private Dictionary<int, double> GTP8_DB;
        private Dictionary<int, double> GTP9_DB;
        private Dictionary<int, double> GTP10_DB;


        private ArrayList TimeReperPoints1;
        private ArrayList TimeReperPoints2;
        private ArrayList TimeReperPoints3;
        private ArrayList TimeReperPoints4;
        private ArrayList TimeReperPoints5;
        private ArrayList TimeReperPoints6;
        private ArrayList TimeReperPoints7;
        private ArrayList TimeReperPoints8;
        private ArrayList TimeReperPoints9;
        private ArrayList TimeReperPoints10;

        private Dictionary<int, double> PGES;
        private Dictionary<int, double> PGTP1;
        private Dictionary<int, double> PGTP2;
        private Dictionary<int, double> PGTP3;
        private Dictionary<int, double> PGTP4;
        private Dictionary<int, double> PGTP5;
        private Dictionary<int, double> PGTP6;
        private Dictionary<int, double> PGTP7;
        private Dictionary<int, double> PGTP8;
        private Dictionary<int, double> PGTP9;
        private Dictionary<int, double> PGTP10;

        private int pbrY; //Год в ПБР
        private int pbrM; //Месяц в ПБР
        private int pbrD; //День в ПБР
        private int pbrHour; //Час в ПБР
        private int pbrMin; //Минута в ПБР
        private int CurrSec = 0;// текущая секунда из Овации
        private int CurrMin = 0;// текущая секунда из Овации
        private int CurrHour = 0;// текущая секунда из Овации
        private int CurrDay = 0;// текущий день из Овации
        private int CurrMonth = 0;// текущий месяц из Овации
        private int CurrYear = 0;// текущий год из Овации
        private int Today;// текущий день

        private Vyrabotka vyr0 = new Vyrabotka();
        private Vyrabotka vyr1 = new Vyrabotka();
        private Vyrabotka vyr2 = new Vyrabotka();
        private Vyrabotka vyr3 = new Vyrabotka();
        private Vyrabotka vyr4 = new Vyrabotka();
        private Vyrabotka vyr5 = new Vyrabotka();
        private Vyrabotka vyr6 = new Vyrabotka();
        private Vyrabotka vyr7 = new Vyrabotka();
        private Vyrabotka vyr8 = new Vyrabotka();
        private Vyrabotka vyr9 = new Vyrabotka();
        private Vyrabotka vyr10 = new Vyrabotka();

        private string IniFileName = "AutoOper.ini";
        private string PbrDirName;
        private string ArchDirName;
        private string LogFileName;
        private double ForbiddenZone;
        private bool isIniFile = false;
        private bool isActualDate = false; //получили актуальное время
        private bool pbrWithData = false;  // есть ли актуальные данные в ПБРе 
        private bool getDBdata = false;    // получили данные из БД
        private bool Permit = false;       //разрешение на редактирование параметров интерполяции   
        private bool isDateChanged = false;    // флаг смены даты
        private bool isGTP1 = false;    // наличие ГТП-1
        private bool isGTP2 = false;    // наличие ГТП-2
        private bool isGTP3 = false;    // наличие ГТП-3
        private bool isGTP4 = false;    // наличие ГТП-4
        private bool isGTP5 = false;    // наличие ГТП-5
        private bool isGTP6 = false;    // наличие ГТП-6
        private bool isGTP7 = false;    // наличие ГТП-7
        private bool isGTP8 = false;    // наличие ГТП-8
        private bool isGTP9 = false;    // наличие ГТП-9
        private bool isGTP10 = false;    // наличие ГТП-10
        public bool isHHGrahp = false;

        private int GTP1_ID;    // ID ГТП-1
        private int GTP2_ID;    // ID ГТП-2
        private int GTP3_ID;    // ID ГТП-3
        private int GTP4_ID;    // ID ГТП-4
        private int GTP5_ID;    // ID ГТП-5
        private int GTP6_ID;    // ID ГТП-6
        private int GTP7_ID;    // ID ГТП-7
        private int GTP8_ID;    // ID ГТП-8
        private int GTP9_ID;    // ID ГТП-9
        private int GTP10_ID;    // ID ГТП-10

        private int SUMGTPID;
        private string[] SUMGTPKEYS;
        private bool SUMGTPCREATE;
        private bool SUMGTPADDITIONAL = true;
        private bool NoConnect = true;

        private int LocalName;    //имя компа на котором запущен АвтоОператор
        private int RUSATime;   //время РУСА в секундах
        private int TimeStep;   //шаг по времени
        private int PowerStep;  //шаг по мощности
        private double GTPIsOk = 1;//статус АвтоОператора
        private Ini.IniFile ini;
        private SqlConnection DBConn;
        private string DBIP;
        private string DBLogin;
        private string DBPassword;
        private string DBName;
        Form f2;

        public Form1() {
            this.InitializeComponent();
            DirectoryInfo info = new DirectoryInfo(Application.StartupPath);
            foreach (FileInfo info2 in info.GetFiles()) {
                if (info2.Name == this.IniFileName) {
                    this.isIniFile = true;
                }
            }
            if (this.isIniFile) {
                this.ini = new Ini.IniFile(Application.StartupPath + @"\" + this.IniFileName);
                string logPath = ini.IniReadValue("Paths", "LOGPATH");
                Logger.InitFileLogger(logPath, "autoOper", new Logger());
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg4);
        }

        private void button10_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg2);
        }

        private void button11_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg3);
        }

        private void button13_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg6);
        }

        private void button15_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg7);
        }

        private void button17_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg8);
        }

        private void button19_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg9);
        }

        private void button21_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg10);
        }

        private void button23_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg0);
        }

        private void button25_Click(object sender, EventArgs e) {
            this.f2 = new Form2();
            this.f2.Owner = this;
            this.f2.Show();
        }

        private void button26_Click(object sender, EventArgs e) {
            if (this.isGTP1) {
                this.UpdateGraph(this.zg1, this.GTP1, this.PGTP1);
            }
            if (this.isGTP2) {
                this.UpdateGraph(this.zg2, this.GTP2, this.PGTP2);
            }
            if (this.isGTP3) {
                this.UpdateGraph(this.zg3, this.GTP3, this.PGTP3);
            }
            if (this.isGTP4) {
                this.UpdateGraph(this.zg4, this.GTP4, this.PGTP4);
            }
            if (this.isGTP5) {
                this.UpdateGraph(this.zg5, this.GTP5, this.PGTP5);
            }
            if (this.isGTP6) {
                this.UpdateGraph(this.zg6, this.GTP6, this.PGTP6);
            }
            if (this.isGTP7) {
                this.UpdateGraph(this.zg7, this.GTP7, this.PGTP7);
            }
            if (this.isGTP8) {
                this.UpdateGraph(this.zg8, this.GTP8, this.PGTP8);
            }
            if (this.isGTP9) {
                this.UpdateGraph(this.zg9, this.GTP9, this.PGTP9);
            }
            if (this.isGTP10) {
                this.UpdateGraph(this.zg10, this.GTP10, this.PGTP10);
            }
            this.UpdateGraph(this.zg0, this.GTP0, this.PGES);
        }

        private void button4_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg5);
        }

        private void button9_Click(object sender, EventArgs e) {
            this.ClearGraph(this.zg1);
        }

        public void CheckAndCreateDir(string path) {
            if (!Directory.Exists(path)) {
                try {
                    Directory.CreateDirectory(path);
                }
                catch (Exception) {
                    this.WriteLog(this.CurrSec, "Ошибка создания папки.");
                }
            }
        }

        private void ClearGraph(ZedGraphControl zg) {
            zg.GraphPane.CurveList.Clear();
            zg.AxisChange();
            zg.Invalidate();
        }

        private void ClearGraphs(string param) {
            this.ClearGraph(this.zg0);
            this.ClearGraph(this.zg1);
            this.ClearGraph(this.zg2);
            this.ClearGraph(this.zg3);
            this.ClearGraph(this.zg4);
            this.ClearGraph(this.zg5);
            this.ClearGraph(this.zg6);
            this.ClearGraph(this.zg7);
            this.ClearGraph(this.zg8);
            this.ClearGraph(this.zg9);
            this.ClearGraph(this.zg10);
            this.PGES.Clear();
            this.PGTP1.Clear();
            this.PGTP2.Clear();
            this.PGTP3.Clear();
            this.PGTP4.Clear();
            this.PGTP5.Clear();
            this.PGTP6.Clear();
            this.PGTP7.Clear();
            this.PGTP8.Clear();
            this.PGTP9.Clear();
            this.PGTP10.Clear();
            for (int i = 0; i < this.GTP0.Length; i++) {
                this.GTP0[i] = 0.0;
            }
            if (this.isGTP1) {
                for (int j = 0; j < this.GTP1.Length; j++) {
                    this.GTP1[j] = 0.0;
                }
                for (int k = 0; k < this.GTP1_final.Length; k++) {
                    this.GTP1_final[k] = 0.0;
                }
            }
            if (this.isGTP2) {
                for (int m = 0; m < this.GTP2.Length; m++) {
                    this.GTP2[m] = 0.0;
                }
                for (int n = 0; n < this.GTP2_final.Length; n++) {
                    this.GTP2_final[n] = 0.0;
                }
            }
            if (this.isGTP3) {
                for (int num6 = 0; num6 < this.GTP3.Length; num6++) {
                    this.GTP3[num6] = 0.0;
                }
                for (int num7 = 0; num7 < this.GTP3_final.Length; num7++) {
                    this.GTP3_final[num7] = 0.0;
                }
            }
            if (this.isGTP4) {
                for (int num8 = 0; num8 < this.GTP4.Length; num8++) {
                    this.GTP4[num8] = 0.0;
                }
                for (int num9 = 0; num9 < this.GTP4_final.Length; num9++) {
                    this.GTP4_final[num9] = 0.0;
                }
            }
            if (this.isGTP5) {
                for (int num10 = 0; num10 < this.GTP5.Length; num10++) {
                    this.GTP5[num10] = 0.0;
                }
                for (int num11 = 0; num11 < this.GTP5_final.Length; num11++) {
                    this.GTP5_final[num11] = 0.0;
                }
            }
            if (this.isGTP6) {
                for (int num12 = 0; num12 < this.GTP6.Length; num12++) {
                    this.GTP6[num12] = 0.0;
                }
                for (int num13 = 0; num13 < this.GTP6_final.Length; num13++) {
                    this.GTP6_final[num13] = 0.0;
                }
            }
            if (this.isGTP7) {
                for (int num14 = 0; num14 < this.GTP7.Length; num14++) {
                    this.GTP7[num14] = 0.0;
                }
                for (int num15 = 0; num15 < this.GTP7_final.Length; num15++) {
                    this.GTP7_final[num15] = 0.0;
                }
            }
            if (this.isGTP8) {
                for (int num16 = 0; num16 < this.GTP8.Length; num16++) {
                    this.GTP8[num16] = 0.0;
                }
                for (int num17 = 0; num17 < this.GTP8_final.Length; num17++) {
                    this.GTP8_final[num17] = 0.0;
                }
            }
            if (this.isGTP9) {
                for (int num18 = 0; num18 < this.GTP9.Length; num18++) {
                    this.GTP9[num18] = 0.0;
                }
                for (int num19 = 0; num19 < this.GTP9_final.Length; num19++) {
                    this.GTP9_final[num19] = 0.0;
                }
            }
            if (this.isGTP10) {
                for (int num20 = 0; num20 < this.GTP10.Length; num20++) {
                    this.GTP10[num20] = 0.0;
                }
                for (int num21 = 0; num21 < this.GTP10_final.Length; num21++) {
                    this.GTP10_final[num21] = 0.0;
                }
            }
        }

        private void ClearServThread() {
            Logger.Info("ClearServThread");
            if (this.MySlave.Listener != null) {
                this.MySlave.Listener.Stop();
                this.MySlave.Listener = null;
            }
            this.ServThread = new Thread(new ThreadStart(this.ServStart));
            this.ServThread.Priority = ThreadPriority.Highest;
            this.ServThread.IsBackground = true;
            Thread.Sleep(0);
            this.ServThread.Start();
            Logger.Info("=finish ClearServThread");
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e) {
            if (this.Permit) {
                switch (this.comboBox1.SelectedIndex) {
                    case 0:
                        this.comboBox2.Enabled = true;
                        this.comboBox4.Enabled = false;
                        return;

                    case 1:
                        this.comboBox2.Enabled = false;
                        this.comboBox4.Enabled = true;
                        return;

                    case 2:
                        this.comboBox2.Enabled = false;
                        this.comboBox4.Enabled = false;
                        break;

                    default:
                        return;
                }
            }
        }

        private void comboBox2_SelectedValueChanged(object sender, EventArgs e) {
            this.TimeStep = Convert.ToInt32(this.comboBox2.Text);
        }

        private void comboBox3_TextChanged_1(object sender, EventArgs e) {
            this.RUSATime = Convert.ToInt32(this.comboBox3.Text);
        }

        private void comboBox4_SelectedValueChanged(object sender, EventArgs e) {
            this.PowerStep = Convert.ToInt32(this.comboBox4.Text);
        }

        private void DBReadData() {
            Logger.Info("DBReadData");
            if (this.isActualDate) {
                this.getDBdata = true;
                try {
                    if (this.isGTP1) {
                        this.DBReadData_each(this.TimeReperPoints1, this.GTP1, this.GTP1_final, this.GTP1_DB, 1);
                    }
                    if (this.isGTP2) {
                        this.DBReadData_each(this.TimeReperPoints2, this.GTP2, this.GTP2_final, this.GTP2_DB, 2);
                    }
                    if (this.isGTP3) {
                        this.DBReadData_each(this.TimeReperPoints3, this.GTP3, this.GTP3_final, this.GTP3_DB, 3);
                    }
                    if (this.isGTP4) {
                        this.DBReadData_each(this.TimeReperPoints4, this.GTP4, this.GTP4_final, this.GTP4_DB, 4);
                    }
                    if (this.isGTP5) {
                        this.DBReadData_each(this.TimeReperPoints5, this.GTP5, this.GTP5_final, this.GTP5_DB, 5);
                    }
                    if (this.isGTP6) {
                        this.DBReadData_each(this.TimeReperPoints6, this.GTP6, this.GTP6_final, this.GTP6_DB, 6);
                    }
                    if (this.isGTP7) {
                        this.DBReadData_each(this.TimeReperPoints7, this.GTP7, this.GTP7_final, this.GTP7_DB, 7);
                    }
                    if (this.isGTP8) {
                        this.DBReadData_each(this.TimeReperPoints8, this.GTP8, this.GTP8_final, this.GTP8_DB, 8);
                    }
                    if (this.isGTP9) {
                        this.DBReadData_each(this.TimeReperPoints9, this.GTP9, this.GTP9_final, this.GTP9_DB, 9);
                    }
                    if (this.isGTP10) {
                        this.DBReadData_each(this.TimeReperPoints10, this.GTP10, this.GTP10_final, this.GTP10_DB, 10);
                    }
                    this.WriteLog(this.CurrSec, "Функции DBReadData_each() выполнена успешно.");

                    this.GTPIsOk = 1.0;
                }
                catch (Exception e) {
                    this.WriteLog(this.CurrSec, "Ошибка выполнения функции DBReadData_each().");
                    Logger.Info(e.ToString());
                    this.GTPIsOk = 0.0;
                }
            }
            this.DrawGTPS();
            Logger.Info("=finish DBReadData");
        }

        private void DBReadData_each(ArrayList TimeReperPoints, double[] GTP, double[] GTP_final, Dictionary<int, double> GTP_DB, int Item) {
            TimeReperPoints.Clear();
            List<int> TRP300 = new List<int>();
            GTP_DB.Clear();
            for (int i = 0; i < 172801; i++) {
                GTP[i] = 0.0;
                GTP_final[i] = 0.0;
            }
            string date = this.GetDate(0);
            SqlCommand command = new SqlCommand("SELECT * FROM DATA WHERE DATA_DATE BETWEEN '" + date + "' AND DATEADD(SECOND,172801,'" + date + "') AND (PARNUMBER = 300 OR PARNUMBER = 301) AND ITEM = " + Convert.ToString(Item) + "ORDER BY DATA_DATE", this.DBConn);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                string str2 = Convert.ToString(reader.GetDateTime(6));
                char[] separator = new char[] { '-', ' ', ':', '.' };
                string[] strArray = str2.Split(separator);
                int num2 = Convert.ToInt32(strArray[0]);
                int num3 = Convert.ToInt32(strArray[1]);
                int num4 = Convert.ToInt32(strArray[2]);
                int num5 = Convert.ToInt32(strArray[3]);
                int num6 = Convert.ToInt32(strArray[4]);
                int num7 = Convert.ToInt32(strArray[5]);
                int key = 0;
                if (((num2 == this.CurrDay) && (num3 == this.CurrMonth)) && (num4 == this.CurrYear)) {
                    key = (num7 + (60 * num6)) + (3600 * num5);
                }
                else if (num2 == this.GetNextDay(this.CurrDay, this.CurrMonth, this.CurrYear)) {
                    key = ((86400 + num7) + (60 * num6)) + (3600 * num5);
                }
                else /*if ((num2 == this.GetNextDay(this.GetNextDay(this.CurrDay, this.CurrMonth, this.CurrYear), this.CurrMonth, this.CurrYear)) && (this.pbrHour <= 2)) */ {
                    key = 172800;
                }
                if (reader.GetInt32(0) == 300) {
                    if (GTP_DB.ContainsKey(key)) {
                        GTP_DB[key] = reader.GetDouble(3);
                    }
                    else {
                        GTP_DB.Add(key, reader.GetDouble(3));
                    }
                    GTP_final[key] = reader.GetDouble(3);
                    if (!TRP300.Contains(key))
                        TRP300.Add(key);
                }
                if (reader.GetInt32(0) == 301) {
                    TimeReperPoints.Add(key);
                    GTP[key] = reader.GetDouble(3);
                }
            }
            reader.Close();
            command.Dispose();
            TimeReperPoints.Sort();
            TRP300.Sort();
            /*from x in GTP_DB
                    orderby x.Key
                    select x;*/
            for (int j = 0; j < (TimeReperPoints.Count - 1); j++) {
                double num10 = (GTP[Convert.ToInt32(TimeReperPoints[j + 1])] - GTP[Convert.ToInt32(TimeReperPoints[j])]) / (Convert.ToDouble(TimeReperPoints[j + 1]) - Convert.ToDouble(TimeReperPoints[j]));
                double num11 = GTP[Convert.ToInt32(TimeReperPoints[j])] - (num10 * Convert.ToDouble(TimeReperPoints[j]));
                for (int m = Convert.ToInt32(TimeReperPoints[j]); m < Convert.ToInt32(TimeReperPoints[j + 1]); m++) {
                    GTP[m] = (num10 * m) + num11;
                }
            }
            int[] array = new int[GTP_DB.Count];
            GTP_DB.Keys.CopyTo(array, 0);
            for (int k = 0; k < (GTP_DB.Count - 1); k++) {
                for (int n = array[k]; n < array[k + 1]; n++) {
                    GTP_final[n] = GTP_final[array[k]];
                }
            }
            if (TimeReperPoints.Count > 0) {
                if (Convert.ToInt32(TimeReperPoints[TimeReperPoints.Count - 1]) < 172800) {
                    for (int num15 = Convert.ToInt32(TimeReperPoints[TimeReperPoints.Count - 1]); num15 < 172800; num15++) {
                        GTP_final[num15 + 1] = GTP_final[num15];
                    }
                }
            }
            Logger.Info("HH:" + isHHGrahp.ToString());
            if (isHHGrahp) {
                if (TRP300.Count > 0) {
                    int first = TRP300.First();
                    //Logger.Info("First: " + first+"  "+GTP_final[first]);
                    for (int s = 0; s < first; s++) {
                        GTP_final[s] = GTP_final[first];
                    }
                    try {
                        if (GTP_final[CurrSec] == 0) { 
                        first = TRP300.First(s => s >= CurrSec);
                        if (first > CurrSec) {
                            for (int s = CurrSec; s <= first; s++) {
                                GTP_final[s] = GTP_final[first];
                            }
                        }
                            }
                    }
                    catch (Exception e) { }
                }
                if (TimeReperPoints.Count > 0) {
                    int first = (int)TimeReperPoints[0];
                    //Logger.Info("First: " + first + "  " + GTP_final[first]);
                    for (int s = 0; s < first; s++) {
                        GTP[s] = GTP[first];
                    }
                }
            }


            TimeReperPoints.Clear();
            GTP_DB.Clear();
        }

        private void DBReadTRP() {
            Logger.Info("DBReadTRP");
            if (this.isActualDate) {
                try {
                    if (this.isGTP1) {
                        this.DBReadTRP_each(this.TimeReperPoints1, this.GTP1, this.GTP1_final, 1);
                    }
                    if (this.isGTP2) {
                        this.DBReadTRP_each(this.TimeReperPoints2, this.GTP2, this.GTP2_final, 2);
                    }
                    if (this.isGTP3) {
                        this.DBReadTRP_each(this.TimeReperPoints3, this.GTP3, this.GTP3_final, 3);
                    }
                    if (this.isGTP4) {
                        this.DBReadTRP_each(this.TimeReperPoints4, this.GTP4, this.GTP4_final, 4);
                    }
                    if (this.isGTP5) {
                        this.DBReadTRP_each(this.TimeReperPoints5, this.GTP5, this.GTP5_final, 5);
                    }
                    if (this.isGTP6) {
                        this.DBReadTRP_each(this.TimeReperPoints6, this.GTP6, this.GTP6_final, 6);
                    }
                    if (this.isGTP7) {
                        this.DBReadTRP_each(this.TimeReperPoints7, this.GTP7, this.GTP7_final, 7);
                    }
                    if (this.isGTP8) {
                        this.DBReadTRP_each(this.TimeReperPoints8, this.GTP8, this.GTP8_final, 8);
                    }
                    if (this.isGTP9) {
                        this.DBReadTRP_each(this.TimeReperPoints9, this.GTP9, this.GTP9_final, 9);
                    }
                    if (this.isGTP10) {
                        this.DBReadTRP_each(this.TimeReperPoints10, this.GTP10, this.GTP10_final, 10);
                    }
                    this.WriteLog(this.CurrSec, "Функция DBReadTRP_each() выполнена успешно.");
                    this.GTPIsOk = 1.0;
                }
                catch (Exception e) {
                    this.WriteLog(this.CurrSec, "Ошибка выполнения функции DBReadTRP_each().");
                    Logger.Info(e.ToString());
                    this.GTPIsOk = 0.0;
                }
            }
            Logger.Info("=finish DBReadTRP");
        }

        private void DBReadTRP_each(ArrayList TimeReperPoints, double[] GTP, double[] GTP_final, int Item) {
            TimeReperPoints.Clear();
            for (int i = 0; i < 172801; i++) {
                GTP[i] = 0.0;
                GTP_final[i] = 0.0;
            }
            string date = this.GetDate(0);
            SqlCommand command = new SqlCommand("SELECT * FROM DATA WHERE DATA_DATE BETWEEN '" + date + "' AND DATEADD(HOUR,48,'" + date + "') AND (PARNUMBER = 301) AND ITEM = " + Convert.ToString(Item) + "ORDER BY DATA_DATE", this.DBConn);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                string str2 = Convert.ToString(reader.GetDateTime(6));
                char[] separator = new char[] { '-', ' ', ':', '.' };
                string[] strArray = str2.Split(separator);
                int num2 = Convert.ToInt32(strArray[0]);
                int num3 = Convert.ToInt32(strArray[1]);
                int num4 = Convert.ToInt32(strArray[2]);
                int num5 = Convert.ToInt32(strArray[3]);
                int num6 = Convert.ToInt32(strArray[4]);
                int num7 = Convert.ToInt32(strArray[5]);
                int num8 = 0;
                if (((num2 == this.CurrDay) && (num3 == this.CurrMonth)) && (num4 == this.CurrYear)) {
                    num8 = (num7 + (60 * num6)) + (3600 * num5);
                }
                else if (num2 == this.GetNextDay(this.CurrDay, this.CurrMonth, this.CurrYear)) {
                    num8 = ((86400 + num7) + (60 * num6)) + (3600 * num5);
                }
                else /*if ((num2 == this.GetNextDay(this.GetNextDay(this.CurrDay, this.CurrMonth, this.CurrYear), this.CurrMonth, this.CurrYear)) && (this.pbrHour <= 2))*/ {
                    num8 = 172800;
                }
                TimeReperPoints.Add(num8);
                GTP[num8] = reader.GetDouble(3);
                GTP_final[num8] = reader.GetDouble(3);
            }
            reader.Close();
            command.Dispose();
            TimeReperPoints.Sort();
            for (int j = 0; j < (TimeReperPoints.Count - 1); j++) {
                double num10 = (GTP[Convert.ToInt32(TimeReperPoints[j + 1])] - GTP[Convert.ToInt32(TimeReperPoints[j])]) / (Convert.ToDouble(TimeReperPoints[j + 1]) - Convert.ToDouble(TimeReperPoints[j]));
                double num11 = GTP[Convert.ToInt32(TimeReperPoints[j])] - (num10 * Convert.ToDouble(TimeReperPoints[j]));
                for (int k = Convert.ToInt32(TimeReperPoints[j]); k < Convert.ToInt32(TimeReperPoints[j + 1]); k++) {
                    GTP[k] = (num10 * k) + num11;
                }
            }
        }

        private void DBWriteData() {
            Logger.Info("DBWriteData");
            if (this.isActualDate) {
                int currSec = this.CurrSec;
                try {
                    if (this.isGTP1) {
                        this.DBWriteData_each(currSec, this.GTP1_DB, this.TimeReperPoints1, this.GTP1, 1);
                    }
                    if (this.isGTP2) {
                        this.DBWriteData_each(currSec, this.GTP2_DB, this.TimeReperPoints2, this.GTP2, 2);
                    }
                    if (this.isGTP3) {
                        this.DBWriteData_each(currSec, this.GTP3_DB, this.TimeReperPoints3, this.GTP3, 3);
                    }
                    if (this.isGTP4) {
                        this.DBWriteData_each(currSec, this.GTP4_DB, this.TimeReperPoints4, this.GTP4, 4);
                    }
                    if (this.isGTP5) {
                        this.DBWriteData_each(currSec, this.GTP5_DB, this.TimeReperPoints5, this.GTP5, 5);
                    }
                    if (this.isGTP6) {
                        this.DBWriteData_each(currSec, this.GTP6_DB, this.TimeReperPoints6, this.GTP6, 6);
                    }
                    if (this.isGTP7) {
                        this.DBWriteData_each(currSec, this.GTP7_DB, this.TimeReperPoints7, this.GTP7, 7);
                    }
                    if (this.isGTP8) {
                        this.DBWriteData_each(currSec, this.GTP8_DB, this.TimeReperPoints8, this.GTP8, 8);
                    }
                    if (this.isGTP9) {
                        this.DBWriteData_each(currSec, this.GTP9_DB, this.TimeReperPoints9, this.GTP9, 9);
                    }
                    if (this.isGTP10) {
                        this.DBWriteData_each(currSec, this.GTP10_DB, this.TimeReperPoints10, this.GTP10, 10);
                    }
                    this.WriteLog(this.CurrSec, "Функция DBWriteData_each() выполнена успешно.");
                    this.GTPIsOk = 1.0;
                }
                catch (Exception e) {
                    this.WriteLog(this.CurrSec, "Ошибка выполнения функции DBWriteData_each().");
                    Logger.Info(e.ToString());
                    this.GTPIsOk = 0.0;
                }
            }
            Logger.Info("=finish DBWriteData");
        }

        private void DBWriteData_each(int tempSec, Dictionary<int, double> GTP_DB, ArrayList TimeReperPoints, double[] GTP, int Item) {
            string str2 = "0";
            string str6 = "0";
            string str8 = "0";
            string str9 = "GETDATE()";
            string currentSeason = this.GetCurrentSeason();
            int second = (TimeReperPoints.Count > 0) ? Convert.ToInt32(TimeReperPoints[0]) : 0;
            int num2 = (TimeReperPoints.Count > 0) ? Convert.ToInt32(TimeReperPoints[TimeReperPoints.Count - 1]) : 0;
            if (second < tempSec) {
                second = tempSec;
            }
            if (second < num2) {
                string str4;
                string str5;
                string str7;
                string date = this.GetDate(second);
                string str12 = this.GetDate(num2 + 1);
                string str3 = Convert.ToString(Item);
                SqlCommand command = new SqlCommand("DELETE FROM DATA WHERE ITEM = " + str3 + " AND  (PARNUMBER=300 OR PARNUMBER=301) AND (DATA_DATE BETWEEN '" + date + "' AND '" + str12 + "')", this.DBConn);
                command.ExecuteNonQuery();
                string str = "300";
                foreach (KeyValuePair<int, double> pair in GTP_DB) {
                    int key = pair.Key;
                    if (key >= tempSec) {
                        str4 = str5 = Convert.ToString(pair.Value).Replace(',', '.');
                        str7 = this.GetDate(key);
                        command = new SqlCommand("INSERT INTO DATA (PARNUMBER,OBJECT,ITEM,VALUE0,VALUE1,OBJTYPE,DATA_DATE,P2KStatus,RcvStamp,SEASON) VALUES (" + str + "," + str2 + "," + str3 + "," + str4 + "," + str5 + "," + str6 + ",'" + str7 + "'," + str8 + "," + str9 + "," + currentSeason + ")", this.DBConn);
                        command.ExecuteNonQuery();
                    }
                }
                str = "301";
                for (int i = 0; i < TimeReperPoints.Count; i++) {
                    int num5 = Convert.ToInt32(TimeReperPoints[i]);
                    if (num5 >= tempSec) {
                        str4 = str5 = Convert.ToString(GTP[Convert.ToInt32(TimeReperPoints[i])]).Replace(',', '.');
                        str7 = this.GetDate(num5);
                        new SqlCommand("INSERT INTO DATA (PARNUMBER,OBJECT,ITEM,VALUE0,VALUE1,OBJTYPE,DATA_DATE,P2KStatus,RcvStamp,SEASON) VALUES (" + str + "," + str2 + "," + str3 + "," + str4 + "," + str5 + "," + str6 + ",'" + str7 + "'," + str8 + "," + str9 + "," + currentSeason + ")", this.DBConn).ExecuteNonQuery();
                    }
                }
            }
            TimeReperPoints.Clear();
            GTP_DB.Clear();
        }

        private void DBWriteTRP() {
            Logger.Info("DBWriteTRP");
            if (this.isActualDate) {
                try {
                    int currSec = this.CurrSec;
                    if (this.isGTP1) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints1, 1, this.GTP1);
                    }
                    if (this.isGTP2) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints2, 2, this.GTP2);
                    }
                    if (this.isGTP3) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints3, 3, this.GTP3);
                    }
                    if (this.isGTP4) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints4, 4, this.GTP4);
                    }
                    if (this.isGTP5) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints5, 5, this.GTP5);
                    }
                    if (this.isGTP6) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints6, 6, this.GTP6);
                    }
                    if (this.isGTP7) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints7, 7, this.GTP7);
                    }
                    if (this.isGTP8) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints8, 8, this.GTP8);
                    }
                    if (this.isGTP9) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints9, 9, this.GTP9);
                    }
                    if (this.isGTP10) {
                        this.DBWriteTRP_each(currSec, this.TimeReperPoints10, 10, this.GTP10);
                    }
                    this.WriteLog(this.CurrSec, "Функция DBWriteTRP_each() выполнена успешно.");
                    this.GTPIsOk = 1.0;
                }
                catch (Exception e) {
                    this.WriteLog(this.CurrSec, "Ошибка выполнения функции DBWriteTRP_each().");
                    Logger.Info(e.ToString());
                    this.GTPIsOk = 0.0;
                }
            }
            Logger.Info("=finish DBWriteTRP");
        }

        private void DBWriteTRP_each(int tempSec, ArrayList TimeReperPoints, int Item, double[] GTP) {
            string str2 = "0";
            string str6 = "0";
            string str8 = "0";
            string str9 = "GETDATE()";
            string currentSeason = this.GetCurrentSeason();
            int second = Convert.ToInt32((TimeReperPoints.Count > 0) ? TimeReperPoints[0] : 0);
            int num2 = Convert.ToInt32((TimeReperPoints.Count > 0) ? this.TimeReperPoints1[TimeReperPoints.Count - 1] : 0);
            if (second < tempSec) {
                second = tempSec;
            }
            if (second < num2) {
                string date = this.GetDate(second);
                string str12 = this.GetDate(num2);
                string str3 = Convert.ToString(Item);
                SqlCommand command = new SqlCommand("DELETE FROM DATA WHERE  (PARNUMBER=301) AND ITEM = " + str3 + " AND (DATA_DATE BETWEEN '" + date + "' AND '" + str12 + "')", this.DBConn);
                command.ExecuteNonQuery();
                string str = "301";
                for (int i = 0; i < TimeReperPoints.Count; i++) {
                    int num4 = Convert.ToInt32(TimeReperPoints[i]);
                    if (num4 >= tempSec) {
                        string str5;
                        string str4 = str5 = Convert.ToString(GTP[Convert.ToInt32(TimeReperPoints[i])]).Replace(',', '.');
                        string str7 = this.GetDate(num4);
                        string cmd = "INSERT INTO DATA (PARNUMBER,OBJECT,ITEM,VALUE0,VALUE1,OBJTYPE,DATA_DATE,P2KStatus,RcvStamp,SEASON) VALUES (" + str + "," + str2 + "," + str3 + "," + str4 + "," + str5 + "," + str6 + ",'" + str7 + "'," + str8 + "," + str9 + "," + currentSeason + ")";
                        new SqlCommand(cmd, this.DBConn).ExecuteNonQuery();
                    }
                }
            }
        }

        private void DrawGTP(ZedGraphControl zgc, double[] GTP, int GTPNumber, Color col, int DrawFill, int bClear, Dictionary<int, double> PReal, int serIndex, int serIndexFakt) {
            Logger.Info("DrawGTP");
            GraphPane graphPane = null;
            double num = 0.0;
            double num2 = 0.0;
            //zgc.Refresh();
            switch (GTPNumber) {
                case 0:
                    graphPane = this.zg0.GraphPane;
                    break;

                case 1:
                    graphPane = this.zg1.GraphPane;
                    break;

                case 2:
                    graphPane = this.zg2.GraphPane;
                    break;

                case 3:
                    graphPane = this.zg3.GraphPane;
                    break;

                case 4:
                    graphPane = this.zg4.GraphPane;
                    break;

                case 5:
                    graphPane = this.zg5.GraphPane;
                    break;

                case 6:
                    graphPane = this.zg6.GraphPane;
                    break;

                case 7:
                    graphPane = this.zg7.GraphPane;
                    break;

                case 8:
                    graphPane = this.zg8.GraphPane;
                    break;

                case 9:
                    graphPane = this.zg9.GraphPane;
                    break;

                case 10:
                    graphPane = this.zg10.GraphPane;
                    break;
            }
            if (bClear == 1) {
                //zgc.GraphPane.CurveList.Clear();
                graphPane.GraphObjList.Clear();
            }

            LineItem curve = null;
            if (graphPane.CurveList.Count > serIndex) {
                curve = graphPane.CurveList[serIndex] as LineItem;
            }
            else {
                PointPairList list = new PointPairList();
                curve = graphPane.AddCurve("ГТП-" + Convert.ToString(GTPNumber), list, col, SymbolType.None);
                curve.Line.Width = 2;
            }


            graphPane.GraphObjList.Clear();
            switch (GTPNumber) {
                case 0:
                    graphPane.Title.Text = "График нагрузки ГЭС";
                    break;

                case 1:
                    graphPane.Title.Text = this.tabPage2.Text;
                    break;

                case 2:
                    graphPane.Title.Text = this.tabPage3.Text;
                    break;

                case 3:
                    graphPane.Title.Text = this.tabPage4.Text;
                    break;

                case 4:
                    graphPane.Title.Text = this.tabPage5.Text;
                    break;

                case 5:
                    graphPane.Title.Text = this.tabPage6.Text;
                    break;

                case 6:
                    graphPane.Title.Text = this.tabPage7.Text;
                    break;

                case 7:
                    graphPane.Title.Text = this.tabPage8.Text;
                    break;

                case 8:
                    graphPane.Title.Text = this.tabPage9.Text;
                    break;

                case 9:
                    graphPane.Title.Text = this.tabPage10.Text;
                    break;

                case 10:
                    graphPane.Title.Text = this.tabPage11.Text;
                    break;
            }
            graphPane.XAxis.Title.Text = "Время, ч";
            graphPane.YAxis.Title.Text = "Мощность, МВт";
            graphPane.XAxis.Type = AxisType.Linear;
            graphPane.Legend.IsVisible = false;
            int ind = -1;
            for (int i = 0; i < 172801; i += 30) {
                ind++;
                double x = Convert.ToDouble(i) / 3600.0;
                if (curve.Points.Count > ind) {
                    curve.Points[ind].X = x;
                    curve.Points[ind].Y = GTP[i];
                }
                else {
                    curve.AddPoint(new PointPair(x, GTP[i]));
                }

                if (GTP[i] > num) {
                    num = GTP[i];
                }
                if (GTP[i] < num2) {
                    num2 = GTP[i];
                }
            }
            try {
                foreach (KeyValuePair<int, double> pair in PReal) {
                    if (pair.Value > num) {
                        num = pair.Value;
                    }
                    if (pair.Value < num2) {
                        num2 = pair.Value;
                    }
                }
            }
            catch { }
            //LineItem item = graphPane.AddCurve("ГТП-" + Convert.ToString(GTPNumber), points, col, SymbolType.None);
            //item.Line.Width = 2f;
            double num5 = 24.0;
            double num6 = Convert.ToDouble(this.CurrSec) / 3600.0;
            LineObj obj2 = new LineObj(Color.Black, num5, num2, num5, ((num / 50.0) + 1.0) * 50.0);
            LineObj obj3 = new LineObj(Color.Red, num6, num2, num6, ((num / 50.0) + 1.0) * 50.0);
            obj2.Line.Width = 3f;
            obj3.Line.Width = 3f;
            graphPane.GraphObjList.Add(obj2);
            graphPane.GraphObjList.Add(obj3);
            TextObj obj4 = new TextObj("Сегодня", 10.0, graphPane.Rect.Top - (0.1 * graphPane.Rect.Height));
            TextObj obj5 = new TextObj("Завтра", 34.0, graphPane.Rect.Top - (0.1 * graphPane.Rect.Height));
            obj4.FontSpec.Border.IsVisible = true;
            obj5.FontSpec.Border.IsVisible = true;
            obj4.FontSpec.Border.Width = 2f;
            obj5.FontSpec.Border.Width = 2f;
            obj4.ZOrder = ZOrder.A_InFront;
            obj5.ZOrder = ZOrder.A_InFront;
            graphPane.GraphObjList.Add(obj4);
            graphPane.GraphObjList.Add(obj5);
            if (DrawFill == 1) {
                curve.Line.Fill = new Fill(Color.FromArgb(80, Color.LightGray));
            }

            if (serIndexFakt != -1) {
                LineItem curveFakt = null;
                if (graphPane.CurveList.Count > serIndexFakt) {
                    curveFakt = graphPane.CurveList[serIndexFakt] as LineItem;
                }
                else {
                    PointPairList list = new PointPairList();
                    curveFakt = graphPane.AddCurve("Fakt ГТП-" + Convert.ToString(GTPNumber), list, Color.DarkOrange, SymbolType.None);
                    curveFakt.Line.Width = 3;
                }

                try {
                    if (curveFakt.Points.Count > PReal.Count) {
                        curveFakt.Clear();
                    }
                }
                catch { }

                int[] keys = PReal.Keys.ToArray<int>();
                ind = -1;
                for (int j = 0; j < PReal.Count; j++) {
                    ind++;
                    double x = Convert.ToDouble(keys[j]) / 3600.0;
                    if (curveFakt.Points.Count > ind) {
                        curveFakt[ind].X = x;
                        curveFakt[ind].Y = PReal[keys[j]];
                    }
                    else {
                        curveFakt.AddPoint(new PointPair(x, PReal[keys[j]]));
                    }

                    /*

              double num8 = Convert.ToDouble(PReal.Keys.ToArray<int>()[j]) / 3600.0;
              double num9 = Convert.ToDouble(PReal.Keys.ToArray<int>()[j + 1]) / 3600.0;
              if ((j + 1) < this.CurrSec) {
                  LineObj obj6 = new LineObj(Color.Orange, num8, PReal.Values.ToArray<double>()[j], num9, PReal.Values.ToArray<double>()[j + 1]) {
                      Line = { Width = 3f }
                  };
                  graphPane.GraphObjList.Add(obj6);
              }*/
                }
            }
            graphPane.XAxis.Scale.Min = 0.0;
            graphPane.XAxis.Scale.Max = 48.0;
            graphPane.YAxis.Scale.Min = num2;
            graphPane.YAxis.Scale.Max = ((num / 50.0) + 1.0) * 50.0;
            if (num2 == num) {
                graphPane.YAxis.Scale.Max = num2 + 50.0;
            }
            graphPane.XAxis.MinorTic.IsAllTics = true;
            graphPane.XAxis.Scale.MinorStep = 0.5;
            graphPane.XAxis.Scale.MajorStep = 1.0;
            graphPane.XAxis.MajorGrid.IsVisible = true;
            graphPane.XAxis.MajorGrid.DashOn = 10f;
            graphPane.XAxis.MinorGrid.DashOn = 20f;
            graphPane.XAxis.MajorGrid.DashOff = 20f;
            graphPane.XAxis.MinorGrid.DashOff = 20f;
            graphPane.YAxis.MajorGrid.IsVisible = true;
            graphPane.YAxis.MajorGrid.DashOn = 10f;
            graphPane.YAxis.MinorGrid.DashOn = 20f;
            graphPane.YAxis.MajorGrid.DashOff = 20f;
            graphPane.YAxis.MinorGrid.DashOff = 20f;
            graphPane.YAxis.MinorGrid.IsVisible = true;
            graphPane.YAxis.MinorGrid.DashOn = 1f;
            graphPane.YAxis.MinorGrid.DashOff = 2f;
            graphPane.XAxis.MinorGrid.IsVisible = true;
            graphPane.XAxis.MinorGrid.DashOn = 1f;
            graphPane.XAxis.MinorGrid.DashOff = 2f;
            Logger.Info("=finish DrawGTP");
        }

        private void DrawGTPS() {
            Logger.Info("DrawGTPs");
            for (int i = 0; i < 172801; i++) {
                this.GTP0[i] = (((((((((this.isGTP1 ? this.GTP1[i] : 0.0) + (this.isGTP2 ? this.GTP2[i] : 0.0)) + (this.isGTP3 ? this.GTP3[i] : 0.0)) + (this.isGTP4 ? this.GTP4[i] : 0.0)) + (this.isGTP5 ? this.GTP5[i] : 0.0)) + (this.isGTP6 ? this.GTP6[i] : 0.0)) + (this.isGTP7 ? this.GTP7[i] : 0.0)) + (this.isGTP8 ? this.GTP8[i] : 0.0)) + (this.isGTP9 ? this.GTP9[i] : 0.0)) + (this.isGTP10 ? this.GTP10[i] : 0.0);
                this.GTP0_final[i] = (((((((((this.isGTP1 ? this.GTP1_final[i] : 0.0) + (this.isGTP2 ? this.GTP2_final[i] : 0.0)) + (this.isGTP3 ? this.GTP3_final[i] : 0.0)) + (this.isGTP4 ? this.GTP4_final[i] : 0.0)) + (this.isGTP5 ? this.GTP5_final[i] : 0.0)) + (this.isGTP6 ? this.GTP6_final[i] : 0.0)) + (this.isGTP7 ? this.GTP7_final[i] : 0.0)) + (this.isGTP8 ? this.GTP8_final[i] : 0.0)) + (this.isGTP9 ? this.GTP9_final[i] : 0.0)) + (this.isGTP10 ? this.GTP10_final[i] : 0.0);
                if (SUMGTPCREATE && SUMGTPADDITIONAL) {
                    if (isGTP1 && GTP1_ID == SUMGTPID) this.GTP0[i] -= this.GTP1[i];
                    if (isGTP2 && GTP2_ID == SUMGTPID) this.GTP0[i] -= this.GTP2[i];
                    if (isGTP3 && GTP3_ID == SUMGTPID) this.GTP0[i] -= this.GTP3[i];
                    if (isGTP4 && GTP4_ID == SUMGTPID) this.GTP0[i] -= this.GTP4[i];
                    if (isGTP5 && GTP5_ID == SUMGTPID) this.GTP0[i] -= this.GTP5[i];
                    if (isGTP6 && GTP6_ID == SUMGTPID) this.GTP0[i] -= this.GTP6[i];
                    if (isGTP7 && GTP7_ID == SUMGTPID) this.GTP0[i] -= this.GTP7[i];
                    if (isGTP8 && GTP8_ID == SUMGTPID) this.GTP0[i] -= this.GTP8[i];
                    if (isGTP9 && GTP9_ID == SUMGTPID) this.GTP0[i] -= this.GTP9[i];
                    if (isGTP10 && GTP10_ID == SUMGTPID) this.GTP0[i] -= this.GTP10[i];

                    if (isGTP1 && GTP1_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP1_final[i];
                    if (isGTP2 && GTP2_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP2_final[i];
                    if (isGTP3 && GTP3_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP3_final[i];
                    if (isGTP4 && GTP4_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP4_final[i];
                    if (isGTP5 && GTP5_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP5_final[i];
                    if (isGTP6 && GTP6_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP6_final[i];
                    if (isGTP7 && GTP7_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP7_final[i];
                    if (isGTP8 && GTP8_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP8_final[i];
                    if (isGTP9 && GTP9_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP9_final[i];
                    if (isGTP10 && GTP10_ID == SUMGTPID) this.GTP0_final[i] -= this.GTP10_final[i];
                }
            }

            this.DrawGTP(this.zg0, this.GTP0, 0, Color.Blue, 1, 1, this.PGES, 0, 1);
            this.DrawGTP(this.zg0, this.GTP0_final, 0, Color.Red, 1, 1, this.PGES, 2, -1);
            if (this.isGTP1) {
                this.DrawGTP(this.zg1, this.GTP1, 1, Color.Blue, 0, 1, this.PGTP1, 0, 1);
                this.DrawGTP(this.zg1, this.GTP1_final, 1, Color.Red, 1, 0, this.PGTP1, 2, -1);
            }
            if (this.isGTP2) {
                this.DrawGTP(this.zg2, this.GTP2, 2, Color.Blue, 0, 1, this.PGTP2, 0, 1);
                this.DrawGTP(this.zg2, this.GTP2_final, 2, Color.Red, 1, 0, this.PGTP2, 2, -1);
            }
            if (this.isGTP3) {
                this.DrawGTP(this.zg3, this.GTP3, 3, Color.Blue, 0, 1, this.PGTP3, 0, 1);
                this.DrawGTP(this.zg3, this.GTP3_final, 3, Color.Red, 1, 0, this.PGTP3, 2, -1);
            }
            if (this.isGTP4) {
                this.DrawGTP(this.zg4, this.GTP4, 4, Color.Blue, 0, 1, this.PGTP4, 0, 1);
                this.DrawGTP(this.zg4, this.GTP4_final, 4, Color.Red, 1, 0, this.PGTP4, 2, -1);
            }
            if (this.isGTP5) {
                this.DrawGTP(this.zg5, this.GTP5, 5, Color.Blue, 0, 1, this.PGTP5, 0, 1);
                this.DrawGTP(this.zg5, this.GTP5_final, 5, Color.Red, 1, 0, this.PGTP5, 2, -1);
            }
            if (this.isGTP6) {
                this.DrawGTP(this.zg6, this.GTP6, 6, Color.Blue, 0, 1, this.PGTP6, 0, 1);
                this.DrawGTP(this.zg6, this.GTP6_final, 6, Color.Red, 1, 0, this.PGTP6, 2, -1);
            }
            if (this.isGTP7) {
                this.DrawGTP(this.zg7, this.GTP7, 7, Color.Blue, 0, 1, this.PGTP7, 0, 1);
                this.DrawGTP(this.zg7, this.GTP7_final, 7, Color.Red, 1, 0, this.PGTP7, 2, -1);
            }
            if (this.isGTP8) {
                this.DrawGTP(this.zg8, this.GTP8, 8, Color.Blue, 0, 1, this.PGTP8, 0, 1);
                this.DrawGTP(this.zg8, this.GTP8_final, 8, Color.Red, 1, 0, this.PGTP8, 2, -1);
            }
            if (this.isGTP9) {
                this.DrawGTP(this.zg9, this.GTP9, 9, Color.Blue, 0, 1, this.PGTP9, 0, 1);
                this.DrawGTP(this.zg9, this.GTP9_final, 9, Color.Red, 1, 0, this.PGTP9, 2, -1);
            }
            if (this.isGTP10) {
                this.DrawGTP(this.zg10, this.GTP10, 10, Color.Blue, 0, 1, this.PGTP10, 0, 1);
                this.DrawGTP(this.zg10, this.GTP10_final, 10, Color.Red, 1, 0, this.PGTP10, 2, -1);
            }
            tabControl1_SelectedIndexChanged(tabControl1, new EventArgs());
            Logger.Info("=finish DrawGTPs");
        }

        private void FillArrays(int pbrD, int pbrM, int pbrY, ArrayList TimeReperPoints, double[] GTP, double[] GTP_final, double power) {
            Logger.Info("FillArrays");
            if (((pbrD == this.CurrDay) && (pbrM == this.CurrMonth)) && (pbrY == this.CurrYear)) {
                TimeReperPoints.Add((3600 * this.pbrHour) + (60 * this.pbrMin));
                GTP[(3600 * this.pbrHour) + (60 * this.pbrMin)] = power;
                GTP_final[(3600 * this.pbrHour) + (60 * this.pbrMin)] = power;
            }
            if (pbrD == this.GetNextDay(this.CurrDay, this.CurrMonth, this.CurrYear)) {
                TimeReperPoints.Add((86400 + (3600 * this.pbrHour)) + (60 * this.pbrMin));
                GTP[(86400 + (3600 * this.pbrHour)) + (60 * this.pbrMin)] = power;
                GTP_final[(86400 + (3600 * this.pbrHour)) + (60 * this.pbrMin)] = power;
            }
            Logger.Info("=finish FillArrays");
        }




        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (this.ServThread != null) {
                this.ServThread.Abort();
            }
            if (!this.isIniFile) {
                this.ini = new Ini.IniFile(Application.StartupPath + @"\" + this.IniFileName);
            }
            this.ini.IniWriteValue("Approx", "TimeStep", this.comboBox2.Text);
            this.ini.IniWriteValue("Approx", "PowerStep", this.comboBox4.Text);
            this.ini.IniWriteValue("Approx", "SelectedIndex", Convert.ToString(this.comboBox1.SelectedIndex));
            try {
                if (this.DBConn != null) {
                    this.DBConn.Close();
                }
            }
            catch (Exception ex) {
                this.WriteLog(this.CurrSec, "Ошибка завершения работы с базой данных.");
                Logger.Info(ex.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            Logger.Info("Form1_Load");
            this.MySlave = new Slave();
            this.MBRegisterWriteValue(1, 0.0);
            this.ForbiddenZone = Convert.ToDouble(this.textBox1.Text);
            DateTime now = DateTime.Now;
            this.CurrYear = now.Year;
            this.CurrMonth = now.Month;
            this.CurrDay = now.Day;
            this.CurrSec = (now.Second + (60 * now.Minute)) + (3600 * now.Hour);
            this.isActualDate = true;
            this.dataGridView1.Rows.Add(this.MySlave.GetNumberMBRegisters());
            for (int i = 0; i < this.MySlave.GetNumberMBRegisters(); i++) {
                this.dataGridView1.Rows[i].Cells[0].Value = Convert.ToString((int)((40000 + i) + 1));
                this.dataGridView1.Rows[i].Cells[2].Value = "Float";
            }
            this.dataGridView1.Rows[0].Cells[3].Value = "OUT--->Счётчик секунд";
            this.dataGridView1.Rows[2].Cells[3].Value = "IN<----Номер текущей секунды в сутках";
            this.dataGridView1.Rows[4].Cells[3].Value = "IN<----Год";
            this.dataGridView1.Rows[6].Cells[3].Value = "IN<----Месяц";
            this.dataGridView1.Rows[8].Cells[3].Value = "IN<----День";
            this.dataGridView1.Rows[10].Cells[3].Value = "OUT--->ГТП-1";
            this.dataGridView1.Rows[12].Cells[3].Value = "OUT--->ГТП-2";
            this.dataGridView1.Rows[14].Cells[3].Value = "OUT--->ГТП-3";
            this.dataGridView1.Rows[16].Cells[3].Value = "OUT--->ГТП-4";
            this.dataGridView1.Rows[18].Cells[3].Value = "OUT--->ГТП-5";
            this.dataGridView1.Rows[20].Cells[3].Value = "OUT--->ГТП-6";
            this.dataGridView1.Rows[22].Cells[3].Value = "OUT--->ГТП-7";
            this.dataGridView1.Rows[24].Cells[3].Value = "OUT--->ГТП-8";
            this.dataGridView1.Rows[26].Cells[3].Value = "OUT--->ГТП-9";
            this.dataGridView1.Rows[28].Cells[3].Value = "OUT--->ГТП-10";
            this.dataGridView1.Rows[30].Cells[3].Value = "OUT--->ГТП-1 РУСА";
            this.dataGridView1.Rows[32].Cells[3].Value = "OUT--->ГТП-2 РУСА";
            this.dataGridView1.Rows[34].Cells[3].Value = "OUT--->ГТП-3 РУСА";
            this.dataGridView1.Rows[36].Cells[3].Value = "OUT--->ГТП-4 РУСА";
            this.dataGridView1.Rows[38].Cells[3].Value = "OUT--->ГТП-5 РУСА";
            this.dataGridView1.Rows[40].Cells[3].Value = "OUT--->ГТП-6 РУСА";
            this.dataGridView1.Rows[42].Cells[3].Value = "OUT--->ГТП-7 РУСА";
            this.dataGridView1.Rows[44].Cells[3].Value = "OUT--->ГТП-8 РУСА";
            this.dataGridView1.Rows[46].Cells[3].Value = "OUT--->ГТП-9 РУСА";
            this.dataGridView1.Rows[48].Cells[3].Value = "OUT--->ГТП-10 РУСА";
            this.dataGridView1.Rows[50].Cells[3].Value = "IN<----Текущая нагрузка ГЭС";
            this.dataGridView1.Rows[52].Cells[3].Value = "OUT--->Статус ГТП";
            this.dataGridView1.Rows[54].Cells[3].Value = "OUT--->Номер данного АРМа";
            this.dataGridView1.Rows[56].Cells[3].Value = "IN<----Текущая нагрузка ГТП-1";
            this.dataGridView1.Rows[58].Cells[3].Value = "IN<----Текущая нагрузка ГТП-2";
            this.dataGridView1.Rows[60].Cells[3].Value = "IN<----Текущая нагрузка ГТП-3";
            this.dataGridView1.Rows[62].Cells[3].Value = "IN<----Текущая нагрузка ГТП-4";
            this.dataGridView1.Rows[64].Cells[3].Value = "IN<----Текущая нагрузка ГТП-5";
            this.dataGridView1.Rows[66].Cells[3].Value = "IN<----Текущая нагрузка ГТП-6";
            this.dataGridView1.Rows[68].Cells[3].Value = "IN<----Текущая нагрузка ГТП-7";
            this.dataGridView1.Rows[70].Cells[3].Value = "IN<----Текущая нагрузка ГТП-8";
            this.dataGridView1.Rows[72].Cells[3].Value = "IN<----Текущая нагрузка ГТП-9";
            this.dataGridView1.Rows[74].Cells[3].Value = "IN<----Текущая нагрузка ГТП-10";

            this.toolStripStatusLabel2.BackColor = Color.Red;
            this.toolStripStatusLabel2.Text = "Соединение отсутствует";
            this.tabControl1.SelectedIndex = 1;
            this.comboBox1.Enabled = false;
            this.comboBox2.Enabled = false;
            this.comboBox4.Enabled = false;
            this.textBox5.BackColor = Color.White;
            this.textBox6.BackColor = Color.White;
            this.textBox5.ForeColor = Color.Black;
            this.textBox6.ForeColor = Color.Black;
            this.label1.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage3.Top - 30)));
            this.label2.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage3.Top - 30)));
            this.label3.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage4.Top - 30)));
            this.label13.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage5.Top - 30)));
            this.label14.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage6.Top - 30)));
            this.label15.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage7.Top - 30)));
            this.label16.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage8.Top - 30)));
            this.label17.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage9.Top - 30)));
            this.label18.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage10.Top - 30)));
            this.label19.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage11.Top - 30)));
            this.label20.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage12.Top - 30)));
            this.RUSATime = Convert.ToInt32(this.comboBox3.Text);
            DirectoryInfo info = new DirectoryInfo(Application.StartupPath);
            foreach (FileInfo info2 in info.GetFiles()) {
                if (info2.Name == this.IniFileName) {
                    this.isIniFile = true;
                }
            }
            if (this.isIniFile) {
                this.ini = new Ini.IniFile(Application.StartupPath + @"\" + this.IniFileName);
                this.MySlave.PORT = Convert.ToInt32(this.ini.IniReadValue("Connection", "Port"));
                this.MySlave.Slave_ID = Convert.ToUInt16(this.ini.IniReadValue("Connection", "SlaveID"));
                this.DBIP = this.ini.IniReadValue("DataBase", "DBIP");
                this.DBLogin = this.ini.IniReadValue("DataBase", "DBLogin");
                this.DBPassword = this.ini.IniReadValue("DataBase", "DBPassword");
                this.DBName = this.ini.IniReadValue("DataBase", "DBName");
                this.PbrDirName = this.ini.IniReadValue("Paths", "PBR");
                this.ArchDirName = this.ini.IniReadValue("Paths", "ARCH");
                this.LogFileName = this.ini.IniReadValue("Paths", "LOG");
                string logPath = ini.IniReadValue("Paths", "LOGPATH");
                Logger.InitFileLogger(logPath, "autoOper", new Logger());

                this.TimeStep = Convert.ToInt32(this.ini.IniReadValue("Approx", "TimeStep"));
                this.comboBox2.Text = this.ini.IniReadValue("Approx", "TimeStep");
                this.PowerStep = Convert.ToInt32(this.ini.IniReadValue("Approx", "PowerStep"));
                this.comboBox4.Text = this.ini.IniReadValue("Approx", "PowerStep");
                this.comboBox1.SelectedIndex = Convert.ToInt32(this.ini.IniReadValue("Approx", "SelectedIndex"));
                if (this.comboBox1.SelectedIndex == 0 && TimeStep == 1800)
                    this.isHHGrahp = true;
                this.isGTP1 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP1"));
                this.isGTP2 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP2"));
                this.isGTP3 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP3"));
                this.isGTP4 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP4"));
                this.isGTP5 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP5"));
                this.isGTP6 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP6"));
                this.isGTP7 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP7"));
                this.isGTP8 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP8"));
                this.isGTP9 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP9"));
                this.isGTP10 = Convert.ToBoolean(this.ini.IniReadValue("GTP", "GTP10"));
                this.tabPage2.Text = this.ini.IniReadValue("GTPName", "GTP1");
                this.tabPage3.Text = this.ini.IniReadValue("GTPName", "GTP2");
                this.tabPage4.Text = this.ini.IniReadValue("GTPName", "GTP3");
                this.tabPage5.Text = this.ini.IniReadValue("GTPName", "GTP4");
                this.tabPage6.Text = this.ini.IniReadValue("GTPName", "GTP5");
                this.tabPage7.Text = this.ini.IniReadValue("GTPName", "GTP6");
                this.tabPage8.Text = this.ini.IniReadValue("GTPName", "GTP7");
                this.tabPage9.Text = this.ini.IniReadValue("GTPName", "GTP8");
                this.tabPage10.Text = this.ini.IniReadValue("GTPName", "GTP9");
                this.tabPage11.Text = this.ini.IniReadValue("GTPName", "GTP10");
                this.GTP1_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP1_ID"));
                this.GTP2_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP2_ID"));
                this.GTP3_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP3_ID"));
                this.GTP4_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP4_ID"));
                this.GTP5_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP5_ID"));
                this.GTP6_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP6_ID"));
                this.GTP7_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP7_ID"));
                this.GTP8_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP8_ID"));
                this.GTP9_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP9_ID"));
                this.GTP10_ID = Convert.ToInt32(this.ini.IniReadValue("GTPID", "GTP10_ID"));
                try {
                    this.SUMGTPCREATE = Boolean.Parse(this.ini.IniReadValue("GTPSUM", "SUMGTPCREATE"));
                    this.SUMGTPID = Int32.Parse(this.ini.IniReadValue("GTPSUM", "SUMGTPID"));
                    char[] separ = { ';' };
                    this.SUMGTPKEYS = this.ini.IniReadValue("GTPSUM", "SUMGTPKEYS").Split(separ);
                    this.SUMGTPADDITIONAL = Boolean.Parse(this.ini.IniReadValue("GTPSUM", "SUMGTPADDITIONAL"));
                }
                catch {
                    this.SUMGTPCREATE = false;
                    this.SUMGTPADDITIONAL = false;
                }
                this.TimeFromMoskow = Convert.ToInt32(this.ini.IniReadValue("Time", "TimeFromMoskow"));
                this.isAdminRules = Convert.ToBoolean(this.ini.IniReadValue("Rules", "Admin"));
            }
            this.PGES = new Dictionary<int, double>();
            this.PGTP1 = new Dictionary<int, double>();
            this.PGTP2 = new Dictionary<int, double>();
            this.PGTP3 = new Dictionary<int, double>();
            this.PGTP4 = new Dictionary<int, double>();
            this.PGTP5 = new Dictionary<int, double>();
            this.PGTP6 = new Dictionary<int, double>();
            this.PGTP7 = new Dictionary<int, double>();
            this.PGTP8 = new Dictionary<int, double>();
            this.PGTP9 = new Dictionary<int, double>();
            this.PGTP10 = new Dictionary<int, double>();

            Logger.Info("Form1_Load - создание массивов");
            this.GTP0 = new double[172801];
            this.GTP0_final = new double[172801];
            if (this.isGTP1) {
                this.GTP1 = new double[172801];
                this.GTP1_final = new double[172801];
                this.TimeReperPoints1 = new ArrayList();
                this.GTP1_DB = new Dictionary<int, double>();
                this.checkBox1.Checked = true;
            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage2);
            }
            if (this.isGTP2) {
                this.GTP2 = new double[172801];
                this.GTP2_final = new double[172801];
                this.TimeReperPoints2 = new ArrayList();
                this.GTP2_DB = new Dictionary<int, double>();
                this.checkBox2.Checked = true;
            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage3);
            }
            if (this.isGTP3) {
                this.GTP3 = new double[172801];
                this.GTP3_final = new double[172801];
                this.TimeReperPoints3 = new ArrayList();
                this.GTP3_DB = new Dictionary<int, double>();
                this.checkBox3.Checked = true;
            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage4);
            }
            if (this.isGTP4) {
                this.GTP4 = new double[172801];
                this.GTP4_final = new double[172801];
                this.TimeReperPoints4 = new ArrayList();
                this.GTP4_DB = new Dictionary<int, double>();
                this.checkBox4.Checked = true;
            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage5);
            }
            if (this.isGTP5) {
                this.GTP5 = new double[172801];
                this.GTP5_final = new double[172801];
                this.TimeReperPoints5 = new ArrayList();
                this.GTP5_DB = new Dictionary<int, double>();
                this.checkBox5.Checked = true;
            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage6);
            }
            if (this.isGTP6) {
                this.GTP6 = new double[172801];
                this.GTP6_final = new double[172801];
                this.TimeReperPoints6 = new ArrayList();
                this.GTP6_DB = new Dictionary<int, double>();
                this.checkBox6.Checked = true;

            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage7);
            }
            if (this.isGTP7) {
                this.GTP7 = new double[172801];
                this.GTP7_final = new double[172801];
                this.TimeReperPoints7 = new ArrayList();
                this.GTP7_DB = new Dictionary<int, double>();
                this.checkBox7.Checked = true;
            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage8);
            }
            if (this.isGTP8) {
                this.GTP8 = new double[172801];
                this.GTP8_final = new double[172801];
                this.TimeReperPoints8 = new ArrayList();
                this.GTP8_DB = new Dictionary<int, double>();
                this.checkBox8.Checked = true;
            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage9);
            }
            if (this.isGTP9) {
                this.GTP9 = new double[172801];
                this.GTP9_final = new double[172801];
                this.TimeReperPoints9 = new ArrayList();
                this.GTP9_DB = new Dictionary<int, double>();
                this.checkBox9.Checked = true;
            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage10);
            }
            if (this.isGTP10) {
                this.GTP10 = new double[172801];
                this.GTP10_final = new double[172801];
                this.TimeReperPoints10 = new ArrayList();
                this.GTP10_DB = new Dictionary<int, double>();
                this.checkBox10.Checked = true;
            }
            else {
                this.tabControl1.TabPages.Remove(this.tabPage11);
            }

            Logger.Info("Form1_Load - соединение с БД");
            this.DBConn = new SqlConnection("server=" + this.DBIP + ";user id =" + this.DBLogin + ";password=" + this.DBPassword + ";database=" + this.DBName + ";connection timeout=15");
            try {
                this.DBConn.Open();
                new SqlCommand("SET DATEFORMAT ymd", DBConn).ExecuteNonQuery();
                this.WriteLog(this.CurrSec, "Соединение с базой данных успешно установлено.");
            }
            catch {
                this.WriteLog(this.CurrSec, "Ошибка соединения с базой данных.");
                this.GTPIsOk = 0.0;
            }
            if ((Environment.MachineName.StartsWith("drop") || Environment.MachineName.StartsWith("Drop")) || Environment.MachineName.StartsWith("DROP")) {
                this.LocalName = Convert.ToInt32(Environment.MachineName.Substring(4, 3));
            }
            this.timer1.Enabled = true;

            this.UpdateVyr();
            zg1.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg2.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg3.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg4.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg5.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg6.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg7.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg8.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg9.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg10.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            zg0.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XAxis_ScaleFormatEvent);
            Logger.Info("= finish Form1_Load");
        }

        private void Form1_Resize(object sender, EventArgs e) {
            Logger.Info("form resize");
            this.SetSizeZG();
            Logger.Info("=finish form resize");
        }

        private string GetCurrentSeason() {
            DateTime now = DateTime.Now;
            if (!this.isActualDate) {
                return "0";
            }
            bool flag = now.IsDaylightSavingTime();
            int num = this.CurrYear * 2;
            if (flag) {
                num++;
            }
            else if (this.CurrMonth > 6) {
                num += 2;
            }
            return Convert.ToString(num);
        }

        private string GetDate(int second) {
            DateTime dt = new DateTime(this.CurrYear, this.CurrMonth, this.CurrDay).AddSeconds(second);
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }




        private int GetNextDay(int day, int month, int year) {
            DateTime dt = new DateTime(year, month, day);
            return dt.AddDays(1).Day;
        }



        private void GTPOutputToNull() {
            this.vyr0.ToNULL();
            this.vyr1.ToNULL();
            this.vyr2.ToNULL();
            this.vyr3.ToNULL();
            this.vyr4.ToNULL();
            this.vyr5.ToNULL();
            this.vyr6.ToNULL();
            this.vyr7.ToNULL();
            this.vyr8.ToNULL();
            this.vyr9.ToNULL();
            this.vyr10.ToNULL();
            this.PGES.Clear();
            this.PGTP1.Clear();
            this.PGTP2.Clear();
            this.PGTP3.Clear();
            this.PGTP3.Clear();
            this.PGTP4.Clear();
            this.PGTP5.Clear();
            this.PGTP6.Clear();
            this.PGTP7.Clear();
            this.PGTP8.Clear();
            this.PGTP9.Clear();
            this.PGTP10.Clear();
        }



        private void MakeInterpolation() {
            Logger.Info("MakeInterpolation");
            if (this.isGTP1) {
                this.GTP1_DB.Clear();
            }
            if (this.isGTP2) {
                this.GTP2_DB.Clear();
            }
            if (this.isGTP3) {
                this.GTP3_DB.Clear();
            }
            if (this.isGTP4) {
                this.GTP4_DB.Clear();
            }
            if (this.isGTP5) {
                this.GTP5_DB.Clear();
            }
            if (this.isGTP6) {
                this.GTP6_DB.Clear();
            }
            if (this.isGTP7) {
                this.GTP7_DB.Clear();
            }
            if (this.isGTP8) {
                this.GTP8_DB.Clear();
            }
            if (this.isGTP9) {
                this.GTP9_DB.Clear();
            }
            if (this.isGTP10) {
                this.GTP10_DB.Clear();
            }
            try {
                switch (this.comboBox1.SelectedIndex) {
                    case 0:
                        if (this.isGTP1) {
                            this.MakeTimeAppr(this.TimeReperPoints1, this.GTP1_final, this.GTP1_DB);
                        }
                        if (this.isGTP2) {
                            this.MakeTimeAppr(this.TimeReperPoints2, this.GTP2_final, this.GTP2_DB);
                        }
                        if (this.isGTP3) {
                            this.MakeTimeAppr(this.TimeReperPoints3, this.GTP3_final, this.GTP3_DB);
                        }
                        if (this.isGTP4) {
                            this.MakeTimeAppr(this.TimeReperPoints4, this.GTP4_final, this.GTP4_DB);
                        }
                        if (this.isGTP5) {
                            this.MakeTimeAppr(this.TimeReperPoints5, this.GTP5_final, this.GTP5_DB);
                        }
                        if (this.isGTP6) {
                            this.MakeTimeAppr(this.TimeReperPoints6, this.GTP6_final, this.GTP6_DB);
                        }
                        if (this.isGTP7) {
                            this.MakeTimeAppr(this.TimeReperPoints7, this.GTP7_final, this.GTP7_DB);
                        }
                        if (this.isGTP8) {
                            this.MakeTimeAppr(this.TimeReperPoints8, this.GTP8_final, this.GTP8_DB);
                        }
                        if (this.isGTP9) {
                            this.MakeTimeAppr(this.TimeReperPoints9, this.GTP9_final, this.GTP9_DB);
                        }
                        if (this.isGTP10) {
                            this.MakeTimeAppr(this.TimeReperPoints10, this.GTP10_final, this.GTP10_DB);
                        }
                        break;

                    case 1:
                        if (this.isGTP1) {
                            this.MakePowerAppr(this.TimeReperPoints1, this.GTP1_final, this.GTP1_DB);
                        }
                        if (this.isGTP2) {
                            this.MakePowerAppr(this.TimeReperPoints2, this.GTP2_final, this.GTP2_DB);
                        }
                        if (this.isGTP3) {
                            this.MakePowerAppr(this.TimeReperPoints3, this.GTP3_final, this.GTP3_DB);
                        }
                        if (this.isGTP4) {
                            this.MakePowerAppr(this.TimeReperPoints4, this.GTP4_final, this.GTP4_DB);
                        }
                        if (this.isGTP5) {
                            this.MakePowerAppr(this.TimeReperPoints5, this.GTP5_final, this.GTP5_DB);
                        }
                        if (this.isGTP6) {
                            this.MakePowerAppr(this.TimeReperPoints6, this.GTP6_final, this.GTP6_DB);
                        }
                        if (this.isGTP7) {
                            this.MakePowerAppr(this.TimeReperPoints7, this.GTP7_final, this.GTP7_DB);
                        }
                        if (this.isGTP8) {
                            this.MakePowerAppr(this.TimeReperPoints8, this.GTP8_final, this.GTP8_DB);
                        }
                        if (this.isGTP9) {
                            this.MakePowerAppr(this.TimeReperPoints9, this.GTP9_final, this.GTP9_DB);
                        }
                        if (this.isGTP10) {
                            this.MakePowerAppr(this.TimeReperPoints10, this.GTP10_final, this.GTP10_DB);
                        }
                        break;

                    case 2:
                        if (this.isGTP1) {
                            this.MakeLinearAppr(this.TimeReperPoints1, this.GTP1_final, this.GTP1_DB);
                        }
                        if (this.isGTP2) {
                            this.MakeLinearAppr(this.TimeReperPoints2, this.GTP2_final, this.GTP2_DB);
                        }
                        if (this.isGTP3) {
                            this.MakeLinearAppr(this.TimeReperPoints3, this.GTP3_final, this.GTP3_DB);
                        }
                        if (this.isGTP4) {
                            this.MakeLinearAppr(this.TimeReperPoints4, this.GTP4_final, this.GTP4_DB);
                        }
                        if (this.isGTP5) {
                            this.MakeLinearAppr(this.TimeReperPoints5, this.GTP5_final, this.GTP5_DB);
                        }
                        if (this.isGTP6) {
                            this.MakeLinearAppr(this.TimeReperPoints6, this.GTP6_final, this.GTP6_DB);
                        }
                        if (this.isGTP7) {
                            this.MakeLinearAppr(this.TimeReperPoints7, this.GTP7_final, this.GTP7_DB);
                        }
                        if (this.isGTP8) {
                            this.MakeLinearAppr(this.TimeReperPoints8, this.GTP8_final, this.GTP8_DB);
                        }
                        if (this.isGTP9) {
                            this.MakeLinearAppr(this.TimeReperPoints9, this.GTP9_final, this.GTP9_DB);
                        }
                        if (this.isGTP10) {
                            this.MakeLinearAppr(this.TimeReperPoints10, this.GTP10_final, this.GTP10_DB);
                        }
                        break;
                }
                this.GTPIsOk = 1.0;
            }
            catch (Exception) {
                this.WriteLog(this.CurrSec, "Ошибка выполнения функции интерполяции.");
                this.GTPIsOk = 0.0;
            }
            Logger.Info("=finish MakeInterpolation");
        }

        private void MakeLinearAppr(ArrayList TimeReperPoints, double[] GTP_final, Dictionary<int, double> GTP_DB) {
            Logger.Info("MakeLinearAppr");
            for (int i = 0; i < (TimeReperPoints.Count - 1); i++) {
                int index = Convert.ToInt32(TimeReperPoints[i]);
                int num4 = Convert.ToInt32(TimeReperPoints[i + 1]);
                double num = (0.5 * Math.Abs((double)(GTP_final[num4] - GTP_final[index]))) * Convert.ToDouble((int)(num4 - index));
                if (GTP_final[index] == GTP_final[num4]) {
                    for (int k = index + 1; k < num4; k++) {
                        GTP_final[k] = GTP_final[index];
                    }
                }
                else if ((GTP_final[index] == 0.0) && (GTP_final[num4] >= this.ForbiddenZone)) {
                    double num7 = 0.5 * (this.ForbiddenZone + GTP_final[num4]);
                    int num6 = Convert.ToInt32(Math.Floor((double)(num4 - (num / num7))));
                    for (int m = index; (m < num6) && (m <= 172800); m++) {
                        GTP_final[m] = 0.0;
                    }
                    double num9 = (GTP_final[num4] - this.ForbiddenZone) / ((double)(num4 - num6));
                    double num10 = this.ForbiddenZone - (num9 * num6);
                    for (int n = num6; (n < num4) && (n <= 172800); n++) {
                        GTP_final[n] = (num9 * n) + num10;
                    }
                }
                else if ((GTP_final[num4] == 0.0) && (GTP_final[index] >= this.ForbiddenZone)) {
                    double num13 = 0.5 * (this.ForbiddenZone + GTP_final[index]);
                    int num12 = Convert.ToInt32(Math.Floor((double)((num / num13) + index)));
                    double num14 = (this.ForbiddenZone - GTP_final[index]) / ((double)(num12 - index));
                    double num15 = GTP_final[index] - (num14 * index);
                    for (int num16 = index; (num16 < num12) && (num16 <= 172800); num16++) {
                        GTP_final[num16] = (num14 * num16) + num15;
                    }
                    for (int num17 = num12; (num17 < num4) && (num17 <= 172800); num17++) {
                        GTP_final[num17] = 0.0;
                    }
                }
                else {
                    double num18 = (GTP_final[num4] - GTP_final[index]) / ((double)(num4 - index));
                    double num19 = GTP_final[index] - (num18 * index);
                    for (int num20 = index; (num20 < num4) && (num20 <= 172800); num20++) {
                        GTP_final[num20] = (num18 * num20) + num19;
                    }
                }
                if (!GTP_DB.ContainsKey(index)) {
                    GTP_DB.Add(index, GTP_final[index]);
                }
                for (int j = index + 1; j < num4; j++) {
                    if (GTP_final[j] != GTP_final[j - 1]) {
                        GTP_DB.Add(j, GTP_final[j]);
                    }
                }
                if (!GTP_DB.ContainsKey(num4)) {
                    GTP_DB.Add(num4, GTP_final[num4]);
                }
            }
            if (Convert.ToInt32(TimeReperPoints[TimeReperPoints.Count - 1]) < 172800) {
                for (int num22 = 1 + Convert.ToInt32(TimeReperPoints[TimeReperPoints.Count - 1]); num22 <= 172800; num22++) {
                    GTP_final[num22] = GTP_final[num22 - 1];
                }
            }
            Logger.Info("=finish MakeLinearAppr");
        }



        private void MakePowerAppr(ArrayList TimeReperPoints, double[] GTP_final, Dictionary<int, double> GTP_DB) {
            Logger.Info("MakePowerAppr");
            for (int i = 0; i < (TimeReperPoints.Count - 1); i++) {
                int index = Convert.ToInt32(TimeReperPoints[i]);
                int num2 = Convert.ToInt32(TimeReperPoints[i + 1]);
                if (GTP_final[index] == GTP_final[num2]) {
                    for (int k = index + 1; k < num2; k++) {
                        GTP_final[k] = GTP_final[index];
                    }
                }
                else {
                    double num5 = Convert.ToDouble(this.PowerStep);
                    int num4 = Convert.ToInt32(Math.Ceiling(Math.Abs((double)((GTP_final[num2] - GTP_final[index]) / num5))));
                    int num3 = Convert.ToInt32(Math.Floor((decimal)((num2 - index) / num4)));
                    Math.Abs((double)(GTP_final[num2] - GTP_final[index]));
                    double num1 = Convert.ToDouble((int)(num2 - index)) / 3600.0;
                    if ((GTP_final[index] == 0.0) && (GTP_final[num2] >= this.ForbiddenZone)) {
                        int num8;
                        int num9 = Convert.ToInt32(Math.Floor((double)((GTP_final[num2] - this.ForbiddenZone) / Math.Abs(num5))));
                        int num10 = num2 - (num9 * num3);
                        double num11 = (GTP_final[num2] - (num9 * num5)) - this.ForbiddenZone;
                        if (num11 == 0.0) {
                            num8 = (index + num10) / 2;
                            for (int m = index; (m < num10) && (m <= 172800); m++) {
                                if (m < num8) {
                                    GTP_final[m] = 0.0;
                                }
                                else {
                                    GTP_final[m] = this.ForbiddenZone;
                                }
                            }
                            for (int n = num10; n < num2; n++) {
                                if (((n - num10) % num3) == 0) {
                                    for (int num14 = n; (num14 < (n + (num3 / 2))) && (num14 <= 172800); num14++) {
                                        GTP_final[num14] = GTP_final[num14 - 1];
                                    }
                                    for (int num15 = n + (num3 / 2); (num15 < (n + num3)) && (num15 <= 172800); num15++) {
                                        GTP_final[num15] = GTP_final[n] + num5;
                                    }
                                }
                            }
                        }
                        else {
                            num8 = ((index + num10) - num3) / 2;
                            for (int num16 = index; (num16 < (num10 - num3)) && (num16 <= 172800); num16++) {
                                if (num16 < num8) {
                                    GTP_final[num16] = 0.0;
                                }
                                else {
                                    GTP_final[num16] = this.ForbiddenZone;
                                }
                            }
                            for (int num17 = num10 - num3; num17 < num10; num17++) {
                                if (((num17 - (num10 - num3)) % num3) == 0) {
                                    for (int num18 = num17; (num18 < (num17 + (num3 / 2))) && (num18 <= 172800); num18++) {
                                        GTP_final[num18] = GTP_final[num18 - 1];
                                    }
                                    for (int num19 = num17 + (num3 / 2); (num19 < (num17 + num3)) && (num19 <= 172800); num19++) {
                                        GTP_final[num19] = GTP_final[num17] + num11;
                                    }
                                }
                            }
                            for (int num20 = num10; num20 < num2; num20++) {
                                if (((num20 - num10) % num3) == 0) {
                                    for (int num21 = num20; (num21 < (num20 + (num3 / 2))) && (num21 <= 172800); num21++) {
                                        GTP_final[num21] = GTP_final[num21 - 1];
                                    }
                                    for (int num22 = num20 + (num3 / 2); (num22 < (num20 + num3)) && (num22 <= 172800); num22++) {
                                        GTP_final[num22] = GTP_final[num20] + num5;
                                    }
                                }
                            }
                        }
                    }
                    else if ((GTP_final[num2] == 0.0) && (GTP_final[index] >= this.ForbiddenZone)) {
                        int num23;
                        int num24 = Convert.ToInt32(Math.Floor((double)((GTP_final[index] - this.ForbiddenZone) / Math.Abs(num5))));
                        double num26 = (GTP_final[index] - (num24 * Math.Abs(num5))) - this.ForbiddenZone;
                        int num25 = index + (num24 * num3);
                        if (num26 == 0.0) {
                            for (int num27 = index + 1; num27 < num25; num27++) {
                                if (((num27 - (index + 1)) % num3) == 0) {
                                    for (int num28 = num27; (num28 < (num27 + (num3 / 2))) && (num28 <= 172800); num28++) {
                                        GTP_final[num28] = GTP_final[num28 - 1];
                                    }
                                    for (int num29 = num27 + (num3 / 2); (num29 < (num27 + num3)) && (num29 <= 172800); num29++) {
                                        GTP_final[num29] = GTP_final[num27] - num5;
                                    }
                                }
                            }
                            num23 = (num2 + num25) / 2;
                            for (int num30 = num25; (num30 < num2) && (num30 <= 172800); num30++) {
                                if (num30 < num23) {
                                    GTP_final[num30] = GTP_final[num30 - 1];
                                }
                                else {
                                    GTP_final[num30] = 0.0;
                                }
                            }
                        }
                        else {
                            for (int num31 = index + 1; num31 < num25; num31++) {
                                if (((num31 - (index + 1)) % num3) == 0) {
                                    for (int num32 = num31; (num32 < (num31 + (num3 / 2))) && (num32 <= 172800); num32++) {
                                        GTP_final[num32] = GTP_final[num32 - 1];
                                    }
                                    for (int num33 = num31 + (num3 / 2); (num33 < (num31 + num3)) && (num33 <= 172800); num33++) {
                                        GTP_final[num33] = GTP_final[num31] - num5;
                                    }
                                }
                            }
                            for (int num34 = num25; num34 < (num25 + num3); num34++) {
                                if (((num34 - num25) % num3) == 0) {
                                    for (int num35 = num34; (num35 < (num34 + (num3 / 2))) && (num35 <= 172800); num35++) {
                                        GTP_final[num35] = GTP_final[num35 - 1];
                                    }
                                    for (int num36 = num34 + (num3 / 2); (num36 < (num34 + num3)) && (num36 <= 172800); num36++) {
                                        GTP_final[num36] = GTP_final[num34] - num26;
                                    }
                                }
                            }
                            num23 = ((num2 + num25) + num3) / 2;
                            for (int num37 = num25 + num3; (num37 < num2) && (num37 <= 172800); num37++) {
                                if (num37 < num23) {
                                    GTP_final[num37] = GTP_final[num37 - 1];
                                }
                                else {
                                    GTP_final[num37] = 0.0;
                                }
                            }
                        }
                    }
                    else {
                        for (int num38 = index + 1; num38 < num2; num38++) {
                            if (((num38 - (index + 1)) % num3) == 0) {
                                if (GTP_final[num2] > GTP_final[index]) {
                                    if ((GTP_final[num38 - 1] + num5) <= GTP_final[num2]) {
                                        for (int num39 = num38; (num39 < (num38 + (num3 / 2))) && (num39 <= 172800); num39++) {
                                            GTP_final[num39] = GTP_final[num39 - 1];
                                        }
                                        for (int num40 = num38 + (num3 / 2); (num40 < (num38 + num3)) && (num40 <= 172800); num40++) {
                                            GTP_final[num40] = GTP_final[num38] + num5;
                                        }
                                    }
                                    else {
                                        for (int num41 = num38; (num41 < (num38 + (num3 / 2))) && (num41 <= 172800); num41++) {
                                            GTP_final[num41] = GTP_final[num41 - 1];
                                        }
                                        for (int num42 = num38 + (num3 / 2); (num42 < (num38 + num3)) && (num42 <= 172800); num42++) {
                                            GTP_final[num42] = GTP_final[num2];
                                        }
                                    }
                                }
                                else if ((GTP_final[num38 - 1] - num5) >= GTP_final[num2]) {
                                    for (long num43 = num38; (num43 < (num38 + (num3 / 2))) && (num43 <= 172800L); num43 += 1L) {
                                        GTP_final[(int)((IntPtr)num43)] = GTP_final[(int)((IntPtr)(num43 - 1L))];
                                    }
                                    for (long num44 = num38 + (num3 / 2); (num44 < (num38 + num3)) && (num44 <= 172800L); num44 += 1L) {
                                        GTP_final[(int)((IntPtr)num44)] = GTP_final[num38] - num5;
                                    }
                                }
                                else {
                                    for (long num45 = num38; (num45 < (num38 + (num3 / 2))) && (num45 <= 172800L); num45 += 1L) {
                                        GTP_final[(int)((IntPtr)num45)] = GTP_final[(int)((IntPtr)(num45 - 1L))];
                                    }
                                    for (long num46 = num38 + (num3 / 2); (num46 < (num38 + num3)) && (num46 <= 172800L); num46 += 1L) {
                                        GTP_final[(int)((IntPtr)num46)] = GTP_final[num2];
                                    }
                                }
                            }
                        }
                    }
                }
                if (!GTP_DB.ContainsKey(index)) {
                    GTP_DB.Add(index, GTP_final[index]);
                }
                for (int j = index + 1; j < num2; j++) {
                    if (GTP_final[j] != GTP_final[j - 1]) {
                        GTP_DB.Add(j, GTP_final[j]);
                    }
                }
                if (!GTP_DB.ContainsKey(num2)) {
                    GTP_DB.Add(num2, GTP_final[num2]);
                }
            }
            if (Convert.ToInt32(TimeReperPoints[TimeReperPoints.Count - 1]) < 172800) {
                for (int num48 = 1 + Convert.ToInt32(TimeReperPoints[TimeReperPoints.Count - 1]); num48 <= 172800; num48++) {
                    GTP_final[num48] = GTP_final[num48 - 1];
                }
            }
            Logger.Info("=finish MakePowerAppr");
        }


        private void MakeHHAppr(ArrayList TimeReperPoints, double[] GTP_final, Dictionary<int, double> GTP_DB) {
            Logger.Info("MakeHHAppr");
            int sec, secDiff, sec1, sec2;
            double p1, p2;
            SortedList<int, double> PBR = new SortedList<int, double>();


            for (int i = 0; i < TimeReperPoints.Count; i++) {
                sec = (int)TimeReperPoints[i];
                secDiff = sec - 15 * 60;
                double p = GTP_final[sec];
                if (p > 10 && p < 35)
                    secDiff = secDiff - 15 * 60;
                secDiff = secDiff < (int)TimeReperPoints[0] ? (int)TimeReperPoints[0] : secDiff;
                PBR.Add(secDiff, p);
                if (i > 0) {
                    sec1 = (int)TimeReperPoints[i - 1];
                    sec2 = (int)TimeReperPoints[i];

                    p1 = GTP_final[sec1];
                    p2 = GTP_final[sec2];
                    if (p1 < 35 && p1 > 10 || p2 < 35 && p2 > 10)
                        continue;
                    sec = (int)(sec2 + sec1) / 2;
                    secDiff = sec - 15 * 60;

                    p = (p1 + p2) / 2;
                    if (p < 35 && p > 10) {
                        int newSec = (int)(p / 35 * 30) * 60;
                        if (p1 < p2) {
                            secDiff = secDiff + 30 * 60 - newSec;
                            PBR.Add(secDiff, 35);
                        }
                        else {
                            PBR.Add(secDiff, 35);
                            PBR.Add(secDiff + newSec, p2);
                        }
                    }
                    else {
                        PBR.Add(secDiff, p);
                    }
                }
            }

            List<int> Keys = PBR.Keys.ToList();
            double prevP = -1;
            int index = 0;
            foreach (int s in Keys) {
                if (index > 0) {
                    if (PBR[s] != prevP)
                        PBR.Add(s - 1, prevP);
                }
                prevP = PBR[s];
                index++;
            }
            PBR.Add((int)TimeReperPoints[TimeReperPoints.Count - 1], PBR.Last().Value);

            prevP = -1;
            sec = 0;
            foreach (KeyValuePair<int, double> de in PBR) {
                GTP_DB.Add(de.Key, de.Value);
                if (sec > 0) {
                    for (int i = sec; i < de.Key; i++) {
                        GTP_final[i] = prevP;
                    }
                }

                prevP = de.Value;
                sec = de.Key;
            }
            for (int s = sec; s <= GTP_final.Count() - 1; s++)
                GTP_final[s] = prevP;

        }

        private void MakeTimeAppr(ArrayList TimeReperPoints, double[] GTP_final, Dictionary<int, double> GTP_DB) {
            if (this.TimeStep == 1800) {
                this.MakeHHAppr(TimeReperPoints, GTP_final, GTP_DB);
                return;
            }

            Logger.Info("MakeTimeAppr");
            for (int i = 0; i < (TimeReperPoints.Count - 1); i++) {
                int index = Convert.ToInt32(TimeReperPoints[i]);
                int num2 = Convert.ToInt32(TimeReperPoints[i + 1]);
                if (GTP_final[index] == GTP_final[num2]) {
                    for (int k = index + 1; k < num2; k++) {
                        GTP_final[k] = GTP_final[index];
                    }
                }
                else {
                    Math.Abs((double)(GTP_final[num2] - GTP_final[index]));
                    double num1 = Convert.ToDouble((int)(num2 - index)) / 3600.0;
                    int timeStep = this.TimeStep;
                    double num4 = (num2 - index) / timeStep;
                    double num3 = (GTP_final[num2] - GTP_final[index]) / num4;
                    if ((GTP_final[index] == 0.0) && (GTP_final[num2] >= this.ForbiddenZone)) {
                        int num8;
                        int num9 = Convert.ToInt32(Math.Floor((double)((GTP_final[num2] - this.ForbiddenZone) / Math.Abs(num3))));
                        int num10 = num2 - (num9 * timeStep);
                        double num11 = (GTP_final[num2] - (num9 * num3)) - this.ForbiddenZone;
                        if (num11 == 0.0) {
                            num8 = (index + num10) / 2;
                            for (int m = index; (m < num10) && (m <= 172800); m++) {
                                if (m < num8) {
                                    GTP_final[m] = 0.0;
                                }
                                else {
                                    GTP_final[m] = this.ForbiddenZone;
                                }
                            }
                            for (int n = num10; n < num2; n++) {
                                if (((n - num10) % timeStep) == 0) {
                                    for (int num14 = n; (num14 < (n + (timeStep / 2))) && (num14 <= 172800); num14++) {
                                        GTP_final[num14] = GTP_final[num14 - 1];
                                    }
                                    for (int num15 = n + (timeStep / 2); (num15 < (n + timeStep)) && (num15 <= 172800); num15++) {
                                        GTP_final[num15] = GTP_final[n] + num3;
                                    }
                                }
                            }
                        }
                        else {
                            num8 = ((index + num10) - timeStep) / 2;
                            for (int num16 = index; (num16 < (num10 - timeStep)) && (num16 <= 172800); num16++) {
                                if (num16 < num8) {
                                    GTP_final[num16] = 0.0;
                                }
                                else {
                                    GTP_final[num16] = this.ForbiddenZone;
                                }
                            }
                            for (int num17 = num10 - timeStep; num17 < num10; num17++) {
                                if (((num17 - (num10 - timeStep)) % timeStep) == 0) {
                                    for (int num18 = num17; (num18 < (num17 + (timeStep / 2))) && (num18 <= 172800); num18++) {
                                        GTP_final[num18] = GTP_final[num18 - 1];
                                    }
                                    for (int num19 = num17 + (timeStep / 2); (num19 < (num17 + timeStep)) && (num19 <= 172800); num19++) {
                                        GTP_final[num19] = GTP_final[num17] + num11;
                                    }
                                }
                            }
                            for (int num20 = num10; num20 < num2; num20++) {
                                if (((num20 - num10) % timeStep) == 0) {
                                    for (int num21 = num20; (num21 < (num20 + (timeStep / 2))) && (num21 <= 172800); num21++) {
                                        GTP_final[num21] = GTP_final[num21 - 1];
                                    }
                                    for (int num22 = num20 + (timeStep / 2); (num22 < (num20 + timeStep)) && (num22 <= 172800); num22++) {
                                        GTP_final[num22] = GTP_final[num20] + num3;
                                    }
                                }
                            }
                        }
                    }
                    else if ((GTP_final[num2] == 0.0) && (GTP_final[index] >= this.ForbiddenZone)) {
                        int num23;
                        int num24 = Convert.ToInt32(Math.Floor((double)((GTP_final[index] - this.ForbiddenZone) / Math.Abs(num3))));
                        double num26 = (GTP_final[index] - (num24 * Math.Abs(num3))) - this.ForbiddenZone;
                        int num25 = index + (num24 * timeStep);
                        if (num26 == 0.0) {
                            for (int num27 = index + 1; num27 < num25; num27++) {
                                if (((num27 - (index + 1)) % timeStep) == 0) {
                                    for (int num28 = num27; (num28 < (num27 + (timeStep / 2))) && (num28 <= 172800); num28++) {
                                        GTP_final[num28] = GTP_final[num28 - 1];
                                    }
                                    for (int num29 = num27 + (timeStep / 2); (num29 < (num27 + timeStep)) && (num29 <= 172800); num29++) {
                                        GTP_final[num29] = GTP_final[num27] + num3;
                                    }
                                }
                            }
                            num23 = (num2 + num25) / 2;
                            for (int num30 = num25; (num30 < num2) && (num30 <= 172800); num30++) {
                                if (num30 < num23) {
                                    GTP_final[num30] = GTP_final[num30 - 1];
                                }
                                else {
                                    GTP_final[num30] = 0.0;
                                }
                            }
                        }
                        else {
                            for (int num31 = index + 1; num31 < num25; num31++) {
                                if (((num31 - (index + 1)) % timeStep) == 0) {
                                    for (int num32 = num31; (num32 < (num31 + (timeStep / 2))) && (num32 <= 172800); num32++) {
                                        GTP_final[num32] = GTP_final[num32 - 1];
                                    }
                                    for (int num33 = num31 + (timeStep / 2); (num33 < (num31 + timeStep)) && (num33 <= 172800); num33++) {
                                        GTP_final[num33] = GTP_final[num31] + num3;
                                    }
                                }
                            }
                            for (int num34 = num25; num34 < (num25 + timeStep); num34++) {
                                if (((num34 - num25) % timeStep) == 0) {
                                    for (int num35 = num34; (num35 < (num34 + (timeStep / 2))) && (num35 <= 172800); num35++) {
                                        GTP_final[num35] = GTP_final[num35 - 1];
                                    }
                                    for (int num36 = num34 + (timeStep / 2); (num36 < (num34 + timeStep)) && (num36 <= 172800); num36++) {
                                        GTP_final[num36] = GTP_final[num34] - num26;
                                    }
                                }
                            }
                            num23 = ((num2 + num25) + timeStep) / 2;
                            for (int num37 = num25 + timeStep; (num37 < num2) && (num37 <= 172800); num37++) {
                                if (num37 < num23) {
                                    GTP_final[num37] = GTP_final[num37 - 1];
                                }
                                else {
                                    GTP_final[num37] = 0.0;
                                }
                            }
                        }
                    }
                    else {
                        for (int num38 = index + 1; num38 < num2; num38++) {
                            if (((num38 - (index + 1)) % timeStep) == 0) {
                                for (int num39 = num38; (num39 < (num38 + (timeStep / 2))) && (num39 <= 172800); num39++) {
                                    GTP_final[num39] = GTP_final[num39 - 1];
                                }
                                for (int num40 = num38 + (timeStep / 2); (num40 < (num38 + timeStep)) && (num40 <= 172800); num40++) {
                                    GTP_final[num40] = GTP_final[num38] + num3;
                                }
                            }
                        }
                    }
                }
                if (!GTP_DB.ContainsKey(index)) {
                    GTP_DB.Add(index, GTP_final[index]);
                }
                for (int j = index + 1; j < num2; j++) {
                    if (GTP_final[j] != GTP_final[j - 1]) {
                        GTP_DB.Add(j, GTP_final[j]);
                    }
                }
                if (!GTP_DB.ContainsKey(num2)) {
                    GTP_DB.Add(num2, GTP_final[num2]);
                }
            }
            if (Convert.ToInt32(TimeReperPoints[TimeReperPoints.Count - 1]) < 172800) {
                for (int num42 = 1 + Convert.ToInt32(TimeReperPoints[TimeReperPoints.Count - 1]); num42 <= 172800; num42++) {
                    GTP_final[num42] = GTP_final[num42 - 1];
                }
            }
            Logger.Info("=finish MakeTimeAppr");
        }



        private float MBRegisterReadValue(int RegNumber) {
            byte[] buffer = new byte[4];
            buffer[3] = this.MySlave.ModbusRegisters[RegNumber - 1].HiByte;
            buffer[2] = this.MySlave.ModbusRegisters[RegNumber - 1].LoByte;
            buffer[1] = this.MySlave.ModbusRegisters[RegNumber].HiByte;
            buffer[0] = this.MySlave.ModbusRegisters[RegNumber].LoByte;
            return BitConverter.ToSingle(buffer, 0);
        }

        private void MBRegisterWriteValue(int RegNumber, double Value) {
            lock (this) {
                byte[] bytes = new byte[4];
                bytes = BitConverter.GetBytes((float)Value);
                this.MySlave.ModbusRegisters[RegNumber - 1].HiByte = bytes[3];
                this.MySlave.ModbusRegisters[RegNumber - 1].LoByte = bytes[2];
                this.MySlave.ModbusRegisters[RegNumber].HiByte = bytes[1];
                this.MySlave.ModbusRegisters[RegNumber].LoByte = bytes[0];
            }
        }

        private void ParsePBR(string PBRFile) {
            Logger.Info("Parse PBR");
            bool readOK = true;
            try {
                string[] strArray = File.ReadAllLines(PBRFile);

                if (SUMGTPCREATE) {
                    Logger.Info("Создание суммарной ГТП");
                    char[] separ = { ';' };
                    Dictionary<int, Dictionary<string, int>> Data = new Dictionary<int, Dictionary<string, int>>();
                    List<string> dates = new List<string>();
                    foreach (string str in strArray) {
                        string[] arr2 = (str + ";").Split(separ);
                        int gtp = Int32.Parse(arr2[0]);
                        string dt = arr2[1];
                        int val = Int32.Parse(arr2[2]);

                        if (!Data.ContainsKey(gtp)) {
                            Data.Add(gtp, new Dictionary<string, int>());
                        }

                        if (!Data[gtp].ContainsKey(dt)) {
                            Data[gtp].Add(dt, val);

                        }
                        else {
                            Data[gtp][dt] = val;
                        }

                        if (!dates.Contains(dt)) {
                            dates.Add(dt);
                        }
                    }
                    dates.Sort();
                    Data.Add(this.SUMGTPID, new Dictionary<string, int>());

                    foreach (string dt in dates) {
                        int val = 0;
                        foreach (string gtp in SUMGTPKEYS) {
                            try {
                                int i = 0;
                                try { i = Int32.Parse(gtp); }
                                catch { }
                                if (i > 0) {
                                    val += Data[i][dt];
                                }
                            }
                            catch {
                                Logger.Info(String.Format("Ошибка при формировании суммарной ГТП. ПБР не принят date={0} gtp={1}", dt, gtp));
                                readOK = false;
                            }
                        }
                        Data[this.SUMGTPID].Add(dt, val);
                    }

                    List<string> listStr = new List<string>();
                    foreach (int gtp in Data.Keys) {
                        if (gtp == GTP1_ID && isGTP1 || gtp == GTP2_ID && isGTP2 || gtp == GTP3_ID && isGTP3 || gtp == GTP4_ID && isGTP4 || gtp == GTP5_ID && isGTP5
                            || gtp == GTP6_ID && isGTP6 || gtp == GTP7_ID && isGTP7 || gtp == GTP8_ID && isGTP8 || gtp == GTP9_ID && isGTP9 || gtp == GTP10_ID && isGTP10) {
                            foreach (string dt in Data[gtp].Keys) {
                                string str = gtp.ToString() + ";" + dt + ";" + Data[gtp][dt].ToString();
                                listStr.Add(str);
                            }
                        }
                    }
                    strArray = listStr.ToArray();
                    if (!readOK)
                        return;
                    Logger.Info("Суммарная ГТП создана");
                }


                if (this.isGTP1) {
                    this.TimeReperPoints1.Clear();
                }
                if (this.isGTP2) {
                    this.TimeReperPoints2.Clear();
                }
                if (this.isGTP3) {
                    this.TimeReperPoints3.Clear();
                }
                if (this.isGTP4) {
                    this.TimeReperPoints4.Clear();
                }
                if (this.isGTP5) {
                    this.TimeReperPoints5.Clear();
                }
                if (this.isGTP6) {
                    this.TimeReperPoints6.Clear();
                }
                if (this.isGTP7) {
                    this.TimeReperPoints7.Clear();
                }
                if (this.isGTP8) {
                    this.TimeReperPoints8.Clear();
                }
                if (this.isGTP9) {
                    this.TimeReperPoints9.Clear();
                }
                if (this.isGTP10) {
                    this.TimeReperPoints10.Clear();
                }
                this.pbrWithData = false;
                foreach (string str in strArray) {
                    char ch = ';';
                    string[] strArray2 = str.Split(new char[] { ch });
                    this.pbrY = Convert.ToInt32(strArray2[1].Substring(0, 4));
                    this.pbrM = Convert.ToInt32(strArray2[1].Substring(4, 2));
                    this.pbrD = Convert.ToInt32(strArray2[1].Substring(6, 2));
                    this.pbrHour = Convert.ToInt32(strArray2[1].Substring(9, 2)) + this.TimeFromMoskow;
                    if (this.pbrHour > 24) {
                        this.pbrHour -= 24;
                        this.pbrD = this.GetNextDay(this.pbrD, this.pbrM, this.pbrY);
                    }
                    this.pbrMin = Convert.ToInt32(strArray2[1].Substring(11, 2));
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP1_ID) && this.isGTP1) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints1, this.GTP1, this.GTP1_final, Convert.ToDouble(strArray2[2]));
                    }
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP2_ID) && this.isGTP2) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints2, this.GTP2, this.GTP2_final, Convert.ToDouble(strArray2[2]));
                    }
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP3_ID) && this.isGTP3) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints3, this.GTP3, this.GTP3_final, Convert.ToDouble(strArray2[2]));
                    }
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP4_ID) && this.isGTP4) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints4, this.GTP4, this.GTP4_final, Convert.ToDouble(strArray2[2]));
                    }
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP5_ID) && this.isGTP5) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints5, this.GTP5, this.GTP5_final, Convert.ToDouble(strArray2[2]));
                    }
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP6_ID) && this.isGTP6) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints6, this.GTP6, this.GTP6_final, Convert.ToDouble(strArray2[2]));
                    }
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP7_ID) && this.isGTP7) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints7, this.GTP7, this.GTP7_final, Convert.ToDouble(strArray2[2]));
                    }
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP8_ID) && this.isGTP8) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints8, this.GTP8, this.GTP8_final, Convert.ToDouble(strArray2[2]));
                    }
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP9_ID) && this.isGTP9) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints9, this.GTP9, this.GTP9_final, Convert.ToDouble(strArray2[2]));
                    }
                    if ((Convert.ToInt32(strArray2[0]) == this.GTP10_ID) && this.isGTP10) {
                        this.FillArrays(this.pbrD, this.pbrM, this.pbrY, this.TimeReperPoints10, this.GTP10, this.GTP10_final, Convert.ToDouble(strArray2[2]));
                    }
                }

                this.WriteLog(this.CurrSec, "Полученный ПБР-файла успешно разобран.");
                if ((strArray.Length != 0) && (((((((((((this.isGTP1 ? this.TimeReperPoints1.Count : 0) != 0) || ((this.isGTP2 ? this.TimeReperPoints2.Count : 0) != 0)) || ((this.isGTP3 ? this.TimeReperPoints3.Count : 0) != 0)) || ((this.isGTP4 ? this.TimeReperPoints4.Count : 0) != 0)) || ((this.isGTP5 ? this.TimeReperPoints5.Count : 0) != 0)) || ((this.isGTP6 ? this.TimeReperPoints6.Count : 0) != 0)) || ((this.isGTP7 ? this.TimeReperPoints7.Count : 0) != 0)) || ((this.isGTP8 ? this.TimeReperPoints8.Count : 0) != 0)) || ((this.isGTP9 ? this.TimeReperPoints9.Count : 0) != 0)) || ((this.isGTP10 ? this.TimeReperPoints10.Count : 0) != 0))) {
                    this.pbrWithData = true;
                }
            }
            catch (Exception e) {
                this.WriteLog(this.CurrSec, "Ошибка функции разбора ПБР-файла.");
                Logger.Info(e.ToString());
            }
            Logger.Info("=finish ParsePBR");
        }

        private void ReadPBR(string f) {
            Logger.Info("ReadPBR");
            this.timer1.Enabled = false;
            this.timer2.Enabled = false;
            this.timer3.Enabled = false;
            this.ParsePBR(f);
            if ((this.pbrWithData && this.isAdminRules) && (this.DBConn.State == ConnectionState.Open)) {
                this.DBWriteTRP();
                //Thread.Sleep(200);
                this.DBReadTRP();
                //Thread.Sleep(200);
                lock (this) {
                    this.MakeInterpolation();
                }
                //Thread.Sleep(200);
                this.DBWriteData();
                this.DBReadData();
                //Thread.Sleep(200);
                this.DrawGTPS();
            }
            this.timer1.Enabled = true;
            this.timer2.Enabled = true;
            this.timer3.Enabled = true;
            Logger.Info("=finish ReadPBR");
        }

        private void ReadPGTPfromDB() {
            Logger.Info("ReadPGTPfromDB");
            string date = this.GetDate(0);
            Logger.Info(date);
            int count = 0;
            SqlCommand command = new SqlCommand("SELECT * FROM DATA WHERE DATA_DATE BETWEEN '" + date + "' AND DATEADD(HOUR,24,'" + date + "') AND (PARNUMBER = 302)ORDER BY DATA_DATE", this.DBConn);
            try {
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    string str2 = Convert.ToString(reader.GetDateTime(6));
                    char[] separator = new char[] { '-', ' ', ':', '.' };
                    string[] strArray = str2.Split(separator);
                    Convert.ToInt32(strArray[0]);
                    Convert.ToInt32(strArray[1]);
                    Convert.ToInt32(strArray[2]);
                    int num = Convert.ToInt32(strArray[3]);
                    int num2 = Convert.ToInt32(strArray[4]);
                    int num4 = (Convert.ToInt32(strArray[5]) + (60 * num2)) + (3600 * num);
                    double num5 = reader.GetDouble(3);
                    if ((reader.GetInt32(2) == 0) && !this.PGES.Keys.Contains<int>(num4)) {
                        this.PGES.Add(num4, num5);
                    }
                    count++;
                }
                reader.Close();
                command.Dispose();
                Logger.Info(String.Format("считано {0} записей ", count));
            }
            catch (Exception e) {
                Logger.Info(e.ToString());
            }
            Logger.Info("=finish ReadPGTPfromDB");
        }

        public void ServStart() {
            if (!this.MySlave.connected) {
                this.MySlave.connect();
            }
            else {
                this.MySlave.disconnect();
            }
        }

        private void SetSizeZG() {
            this.zg0.Location = new Point(11, 34);
            this.zg1.Location = new Point(11, 34);
            this.zg2.Location = new Point(11, 34);
            this.zg3.Location = new Point(11, 34);
            this.zg4.Location = new Point(11, 34);
            this.zg5.Location = new Point(11, 34);
            this.zg6.Location = new Point(11, 34);
            this.zg7.Location = new Point(11, 34);
            this.zg8.Location = new Point(11, 34);
            this.zg9.Location = new Point(11, 34);
            this.zg10.Location = new Point(11, 34);
            this.tabControl1.Size = new Size(base.ClientSize.Width - 5, base.ClientSize.Height - 120);
            this.dataGridView1.Size = new Size(714, this.tabControl1.Height - 50);
            this.label1.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage3.Top - 30)));
            this.label2.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage3.Top - 30)));
            this.label3.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage4.Top - 30)));
            this.label13.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage5.Top - 30)));
            this.label14.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage6.Top - 30)));
            this.label15.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage7.Top - 30)));
            this.label16.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage8.Top - 30)));
            this.label17.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage9.Top - 30)));
            this.label18.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage10.Top - 30)));
            this.label19.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage11.Top - 30)));
            this.label20.Location = new Point(Convert.ToInt32((double)(this.tabControl1.Width * 0.5)), Convert.ToInt32((int)(this.tabPage12.Top - 30)));

            this.zg0.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg1.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg2.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg3.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg4.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg5.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg6.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg7.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg8.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg9.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);
            this.zg10.Size = new Size(this.tabControl1.Width - 20, this.tabControl1.Height - 50);

            tabControl1_SelectedIndexChanged(tabControl1, new EventArgs());
        }

        private void tabControl1_DragDrop(object sender, DragEventArgs e) {
            try {
                Array data = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (data != null) {
                    string f = data.GetValue(0).ToString();
                    this.ReadPBR(f);
                }
            }
            catch (Exception) {
                this.WriteLog(this.CurrSec, "Ошибка обработки Drag&Drop файла на форму.");
            }
        }

        private void tabControl1_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effect = DragDropEffects.Copy;
            }
            else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void textBox4_KeyDown_1(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                this.Permit = this.textBox4.Text == "wdpf";
                this.textBox4.Clear();
            }
            if (!this.Permit) {
                this.comboBox3.Enabled = false;
                this.textBox1.Enabled = false;
                this.comboBox2.Enabled = false;
                this.comboBox4.Enabled = false;
                this.comboBox1.Enabled = false;
                this.tabPage1.IsAccessible = false;
            }
            else {
                this.comboBox3.Enabled = true;
                this.textBox1.Enabled = true;
                switch (this.comboBox1.SelectedIndex) {
                    case 0:
                        this.comboBox2.Enabled = true;
                        this.comboBox4.Enabled = false;
                        break;

                    case 1:
                        this.comboBox2.Enabled = false;
                        this.comboBox4.Enabled = true;
                        break;

                    case 2:
                        this.comboBox2.Enabled = false;
                        this.comboBox4.Enabled = false;
                        break;
                }
                this.comboBox1.Enabled = true;
                this.tabPage1.IsAccessible = true;
                this.WriteLog(this.CurrSec, "Получен доступ к редактированию параметров АвтоОператора.");
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            Logger.Info("timer1_Tick");
            DateTime now = DateTime.Now;
            this.ForbiddenZone = Convert.ToDouble(this.textBox1.Text);
            if (this.MySlave.connected) {
                this.CurrSec = Convert.ToInt32(this.MBRegisterReadValue(3));
                this.CurrYear = Convert.ToInt32(this.MBRegisterReadValue(5));
                this.CurrMonth = Convert.ToInt32(this.MBRegisterReadValue(7));
                this.CurrDay = Convert.ToInt32(this.MBRegisterReadValue(9));
            }
            else {
                this.CurrSec = ((now.Hour * 3600) + (now.Minute * 60)) + now.Second;
                this.CurrYear = now.Year;
                this.CurrMonth = now.Month;
                this.CurrDay = now.Day;
            }
            if (this.CurrSec < 0) {
                this.CurrSec = 0;
            }
            if (this.CurrSec > 172800) {
                this.CurrSec = 172800;
            }
            this.CurrMin = (this.CurrSec % 3600) / 60;
            this.CurrHour = this.CurrSec / 3600;
            if (((this.CurrYear != 0) && (this.CurrMonth != 0)) && ((this.MySlave.connected && this.isAdminRules) || !this.isAdminRules)) {
                this.isActualDate = true;
            }
            else {
                this.isActualDate = false;
            }
            if ((this.CurrYear.ToString().Length <= 2) && this.isActualDate) {
                this.CurrYear += 2000;
            }
            if (this.CurrDay != this.Today) {
                this.isDateChanged = true;
            }
            else {
                this.isDateChanged = false;
            }
            this.Today = this.CurrDay;
            if (((((24 + (this.CurrSec / 3600)) - this.TimeFromMoskow) % 24) != this.MoskowCurrHour) && (((this.CurrSec / 3600) - this.TimeFromMoskow) == 0)) {
                this.PGESyesterdayFACT = 0.0;
                this.PGEStodayFACT = 0.0;
                this.PGESyesterdayPLAN = 0.0;
                this.PGEStodayPLAN = 0.0;
                this.GTPOutputToNull();
                this.PGES.Clear();
            }
            this.MoskowCurrHour = ((24 + (this.CurrSec / 3600)) - this.TimeFromMoskow) % 24;
            if ((this.isDateChanged || !this.getDBdata) && (this.isActualDate && (this.DBConn.State == ConnectionState.Open))) {
                this.PGESyesterdayFACT = this.vyr0.GetFactVyr();
                this.PGEStodayFACT = 0.0;
                this.PGESyesterdayPLAN = this.vyr0.GetPlanVyr();
                this.PGEStodayPLAN = 0.0;
                this.PGES.Clear();
                lock (this) {
                    this.DBReadData();
                }
            }
            if (this.isActualDate) {
                if (!this.isReadPGTPfromDB) {
                    this.ReadPGTPfromDB();
                    this.isReadPGTPfromDB = true;
                }
                if (!this.PGES.Keys.Contains<int>(this.CurrSec)) {
                    this.PGES.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(51)));
                    if (this.isAdminRules) {
                        this.WritePGTPtoDB(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(51)), 0);
                    }
                }
                Logger.Info("timer1_Tick чтение modbus");
                if (!this.PGTP1.Keys.Contains<int>(this.CurrSec) && this.isGTP1) {
                    this.PGTP1.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(57)));
                }
                if (!this.PGTP2.Keys.Contains<int>(this.CurrSec) && this.isGTP2) {
                    this.PGTP2.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(59)));
                }
                if (!this.PGTP3.Keys.Contains<int>(this.CurrSec) && this.isGTP3) {
                    this.PGTP3.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(61)));
                }
                if (!this.PGTP4.Keys.Contains<int>(this.CurrSec) && this.isGTP4) {
                    this.PGTP4.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(63)));
                }
                if (!this.PGTP5.Keys.Contains<int>(this.CurrSec) && this.isGTP5) {
                    this.PGTP5.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(65)));
                }
                if (!this.PGTP6.Keys.Contains<int>(this.CurrSec) && this.isGTP6) {
                    this.PGTP6.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(67)));
                }
                if (!this.PGTP7.Keys.Contains<int>(this.CurrSec) && this.isGTP7) {
                    this.PGTP7.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(69)));
                }
                if (!this.PGTP8.Keys.Contains<int>(this.CurrSec) && this.isGTP8) {
                    this.PGTP8.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(71)));
                }
                if (!this.PGTP9.Keys.Contains<int>(this.CurrSec) && this.isGTP9) {
                    this.PGTP9.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(73)));
                }
                if (!this.PGTP10.Keys.Contains<int>(this.CurrSec) && this.isGTP10) {
                    this.PGTP10.Add(this.CurrSec, Convert.ToDouble(this.MBRegisterReadValue(75)));
                }
            }
            try {
                Logger.Info("timer1_Tick запись modbus");
                if ((this.CurrSec > 10) && this.MySlave.connected) {
                    this.MBRegisterWriteValue(1, (double)((this.MBRegisterReadValue(1) + 1f) % 60f));
                    if (this.isGTP1) {
                        this.MBRegisterWriteValue(11, this.GTP1_final[this.CurrSec]);
                        this.MBRegisterWriteValue(31, this.GTP1_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    if (this.isGTP2) {
                        this.MBRegisterWriteValue(13, this.GTP2_final[this.CurrSec]);
                        this.MBRegisterWriteValue(33, this.GTP2_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    if (this.isGTP3) {
                        this.MBRegisterWriteValue(15, this.GTP3_final[this.CurrSec]);
                        this.MBRegisterWriteValue(35, this.GTP3_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    if (this.isGTP4) {
                        this.MBRegisterWriteValue(17, this.GTP4_final[this.CurrSec]);
                        this.MBRegisterWriteValue(37, this.GTP4_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    if (this.isGTP5) {
                        this.MBRegisterWriteValue(19, this.GTP5_final[this.CurrSec]);
                        this.MBRegisterWriteValue(39, this.GTP5_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    if (this.isGTP6) {
                        this.MBRegisterWriteValue(21, this.GTP6_final[this.CurrSec]);
                        this.MBRegisterWriteValue(41, this.GTP6_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    if (this.isGTP7) {
                        this.MBRegisterWriteValue(23, this.GTP7_final[this.CurrSec]);
                        this.MBRegisterWriteValue(43, this.GTP7_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    if (this.isGTP8) {
                        this.MBRegisterWriteValue(25, this.GTP8_final[this.CurrSec]);
                        this.MBRegisterWriteValue(45, this.GTP8_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    if (this.isGTP9) {
                        this.MBRegisterWriteValue(27, this.GTP9_final[this.CurrSec]);
                        this.MBRegisterWriteValue(47, this.GTP9_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    if (this.isGTP10) {
                        this.MBRegisterWriteValue(29, this.GTP10_final[this.CurrSec]);
                        this.MBRegisterWriteValue(49, this.GTP10_final[(this.CurrSec + this.RUSATime) % 172800]);
                    }
                    this.MBRegisterWriteValue(53, this.GTPIsOk);
                    this.MBRegisterWriteValue(55, (double)this.LocalName);
                }
            }
            catch (Exception) {
                this.WriteLog(this.CurrSec, "Ошибка записи данных в регистры Modbus.");
                this.GTPIsOk = 0.0;
            }
            try {
                for (int i = 1; i < this.MySlave.GetNumberMBRegisters(); i += 2) {
                    this.dataGridView1.Rows[i - 1].Cells[1].Value = Convert.ToString(this.MBRegisterReadValue(i));
                }
            }
            catch {
                this.WriteLog(this.CurrSec, "Ошибка перезаписи регистров Modbus в таблицу.");
            }
            if ((this.ServThread == null) || (!this.MySlave.connected && (this.ServThread != null))) {
                if (this.MySlave.Listener != null) {
                    this.MySlave.Listener.Stop();
                    this.MySlave.Listener = null;
                }
                this.ServThread = new Thread(new ThreadStart(this.ServStart));
                this.ServThread.Priority = ThreadPriority.Highest;
                this.ServThread.IsBackground = true;
                Thread.Sleep(0);
                this.ServThread.Start();
            }
            if (this.MySlave.connected) {
                this.toolStripStatusLabel2.BackColor = Color.Green;
                this.toolStripStatusLabel2.Text = "Соединение установлено";
            }
            else {
                this.toolStripStatusLabel2.BackColor = Color.Red;
                this.toolStripStatusLabel2.Text = "Соединение отсутствует";
            }
            this.label1.Text = this.isGTP1 ? Convert.ToString(Math.Round(this.GTP1_final[this.CurrSec], 1)) : "0";
            this.label2.Text = this.isGTP2 ? Convert.ToString(Math.Round(this.GTP2_final[this.CurrSec], 1)) : "0";
            this.label3.Text = this.isGTP3 ? Convert.ToString(Math.Round(this.GTP3_final[this.CurrSec], 1)) : "0";
            this.label13.Text = this.isGTP4 ? Convert.ToString(Math.Round(this.GTP4_final[this.CurrSec], 1)) : "0";
            this.label14.Text = this.isGTP5 ? Convert.ToString(Math.Round(this.GTP5_final[this.CurrSec], 1)) : "0";
            this.label15.Text = this.isGTP6 ? Convert.ToString(Math.Round(this.GTP6_final[this.CurrSec], 1)) : "0";
            this.label16.Text = this.isGTP7 ? Convert.ToString(Math.Round(this.GTP7_final[this.CurrSec], 1)) : "0";
            this.label17.Text = this.isGTP8 ? Convert.ToString(Math.Round(this.GTP8_final[this.CurrSec], 1)) : "0";
            this.label18.Text = this.isGTP9 ? Convert.ToString(Math.Round(this.GTP9_final[this.CurrSec], 1)) : "0";
            this.label19.Text = this.isGTP10 ? Convert.ToString(Math.Round(this.GTP10_final[this.CurrSec], 1)) : "0";
            this.label20.Text = Convert.ToString(Math.Round(this.GTP0[this.CurrSec], 1));
            Logger.Info("timer1_Tick расчет выработки");
            int[] keys = PGES.Keys.ToArray<int>();
            if ((this.CurrSec > 0) && this.isActualDate) {
                try {
                    this.vyr0.ToNULL();
                    if (this.CurrSec > (this.TimeFromMoskow * 3600)) {
                        for (int j = 0; j < (this.PGES.Keys.Count - 1); j++) {
                            if (keys[j] >= (this.TimeFromMoskow * 3600)) {
                                this.PGEStodayFACT = Math.Abs((double)((0.5 * (((double)(keys[j + 1] - keys[j])) / 3600.0)) * (PGES[keys[j]] + PGES[keys[j + 1]])));
                                this.vyr0.AddFactVyr(this.PGEStodayFACT);
                            }
                        }
                    }
                    else {
                        for (int k = 0; k < (this.PGES.Keys.Count - 1); k++) {
                            if (keys[k + 1] <= (this.TimeFromMoskow * 3600)) {
                                this.PGEStodayFACT = Math.Abs((double)((0.5 * (((double)(keys[k + 1] - keys[k])) / 3600.0)) * (PGES[keys[k]] + PGES[keys[k + 1]])));
                                this.vyr0.AddFactVyr(this.PGEStodayFACT);
                            }
                        }
                        this.vyr0.AddFactVyr(this.PGESyesterdayFACT);
                    }
                    if (this.CurrSec > (this.TimeFromMoskow * 3600)) {
                        for (int m = this.TimeFromMoskow * 3600; m < (this.CurrSec - 1); m++) {
                            this.PGEStodayPLAN = Math.Abs((double)(0.00013888888888888889 * (this.GTP0[m] + this.GTP0[m + 1])));
                            this.vyr0.AddPlanVyr(this.PGEStodayPLAN);
                        }
                    }
                    else {
                        for (int n = 0; n < ((this.TimeFromMoskow * 3600) - 1); n++) {
                            this.PGEStodayPLAN = Math.Abs((double)(0.00013888888888888889 * (this.GTP0[n] + this.GTP0[n + 1])));
                            this.vyr0.AddPlanVyr(this.PGEStodayPLAN);
                        }
                        this.vyr0.AddPlanVyr(this.PGESyesterdayPLAN);
                    }
                }
                catch (Exception ex) {
                    Logger.Info(ex.ToString());
                }
            }
            Logger.Info("=finish timer1Tick");
        }



        private void timer2_Tick(object sender, EventArgs e) {
            Logger.Info("Поиск ПБР");
            this.UpdateVyr();
            DirectoryInfo info = new DirectoryInfo(this.PbrDirName);
            DirectoryInfo info2 = new DirectoryInfo(this.ArchDirName);
            this.CheckAndCreateDir(this.PbrDirName);
            this.CheckAndCreateDir(this.ArchDirName);
            foreach (FileInfo info3 in info.GetFiles("*.csv")) {
                if (info3.Length == 0) {
                    info3.Delete();
                }
                else {
                    this.ReadPBR(info3.FullName);
                    try {
                        string str = info3.Name;
                        if (str.Contains("_parsed_")) {
                            str = str.Remove(str.IndexOf("_parsed_")) + ".csv";
                        }
                        string destFileName = this.ArchDirName + @"\" + str.Replace(".csv", "_parsed_" + DateTime.Now.ToString("dd_MM_HH_mm_ss") + ".csv");
                        info3.MoveTo(destFileName);
                    }
                    catch (Exception) {
                        this.WriteLog(this.CurrSec, "Ошибка работы с *.csv файлом.");
                    }
                }
            }
            Logger.Info("=finish Поиск ПБР");
        }

        private void timer3_Tick(object sender, EventArgs e) {
            if ((this.isActualDate && !this.isAdminRules) && (this.DBConn.State == ConnectionState.Open)) {
                lock (this) {
                    this.DBReadData();
                }
            }
            else {
                this.DrawGTPS();
            }
        }

        private void timer4_Tick(object sender, EventArgs e) {
            DateTime now = DateTime.Now;
            this.textBox6.Text = Convert.ToString(now.Day).PadLeft(2, '0') + "/" + Convert.ToString(now.Month).PadLeft(2, '0') + "/" + Convert.ToString(now.Year).PadLeft(2, '0');
            this.textBox5.Text = Convert.ToString(now.Hour).PadLeft(2, '0') + ":" + Convert.ToString(now.Minute).PadLeft(2, '0') + ":" + Convert.ToString(now.Second).PadLeft(2, '0');
        }

        private void UpdateGraph(ZedGraphControl zg, double[] GTP, Dictionary<int, double> PReal) {
            double num = 0.0;
            double num2 = 0.0;
            for (int i = 1; i < 172801; i++) {
                if (GTP[i] > num) {
                    num = GTP[i];
                }
                if (GTP[i] < num2) {
                    num2 = GTP[i];
                }
            }
            foreach (KeyValuePair<int, double> pair in PReal) {
                if (pair.Value > num) {
                    num = pair.Value;
                }
                if (pair.Value < num2) {
                    num2 = pair.Value;
                }
            }
            zg.GraphPane.XAxis.Scale.Min = 0.0;
            zg.GraphPane.XAxis.Scale.Max = 48.0;
            zg.GraphPane.YAxis.Scale.Min = num2;
            zg.GraphPane.YAxis.Scale.Max = ((num / 50.0) + 1.0) * 50.0;
            if (num2 == num) {
                zg.GraphPane.YAxis.Scale.Max = num2 + 50.0;
            }
            zg.AxisChange();
            zg.Invalidate();
        }


        public void UpdateVyr() {
            this.label32.Text = "Выработка ПЛАН = " + Convert.ToString(Math.Round(this.vyr0.GetPlanVyr(), 1)) + " МВт*ч\nВыработка ФАКТ = " + Convert.ToString(Math.Round(this.vyr0.GetFactVyr(), 1)) + " МВт*ч\nРазница: " + Convert.ToString(Math.Round(this.vyr0.GetDeltaVyr(), 1)) + " МВт*ч";
            this.label32.Location = new Point(Convert.ToInt32((int)(this.tabPage12.Left + 9)), Convert.ToInt32((int)(this.tabPage12.Top + 5)));
        }

        private void WriteLog(int TimeInSec, string Data) {
            Logger.Info(Data);
            return;
        }

        private void WritePGTPtoDB(int CSec, double Power, int Item) {
            Logger.Info("WritePGTPtoDB");
            string str2 = "0";
            string str6 = "0";
            string str8 = "0";
            string str9 = "GETDATE()";
            string currentSeason = this.GetCurrentSeason();
            string date = this.GetDate(CSec);
            string str3 = Convert.ToString(Item);
            SqlCommand command = new SqlCommand("DELETE FROM DATA WHERE  (PARNUMBER=302) AND ITEM = " + str3 + " AND (DATA_DATE = '" + date + "')", this.DBConn);
            try {
                string str5;
                command.ExecuteNonQuery();
                string str = "302";
                string str4 = str5 = Convert.ToString(Power).Replace(',', '.');
                string str7 = date;
                new SqlCommand("INSERT INTO DATA (PARNUMBER,OBJECT,ITEM,VALUE0,VALUE1,OBJTYPE,DATA_DATE,P2KStatus,RcvStamp,SEASON) VALUES (" + str + "," + str2 + "," + str3 + "," + str4 + "," + str5 + "," + str6 + ",'" + str7 + "'," + str8 + "," + str9 + "," + currentSeason + ")", this.DBConn).ExecuteNonQuery();
            }
            catch (Exception e) {
                Logger.Info("=finish WritePGTPtoDB");
            }
        }

        private string XAxis_ScaleFormatEvent(GraphPane pane, Axis axis, double val, int index) {
            int num = Convert.ToInt32(val);
            if (num < 0) {
                num = 24 + (num % 24);
            }
            else {
                num = num % 24;
            }
            return Convert.ToString(num);
        }



        private string zg0_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg1_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg10_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg2_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg3_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg4_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg5_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg6_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg7_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg8_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }

        private string zg9_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt) {
            PointPair pair = curve[iPt];
            return string.Format("Время: {0}\nМощность: {1:F3}", Convert.ToString(Math.Floor((double)(pair.X % 24.0))).PadLeft(2, '0') + ":" + Convert.ToString(Math.Floor((double)((pair.X % 1.0) * 60.0))).PadLeft(2, '0'), pair.Y);
        }
        //---------------------------------------------------------------------
        public class Vyrabotka {
            private double DeltaVyr = 0.0;
            private double FactVyr = 0.0;
            private double FactVyrToday = 0.0;
            private double FactVyrYesterday = 0.0;
            private double PlanVyr = 0.0;
            private double PlanVyrToday = 0.0;
            private double PlanVyrYesterday = 0.0;

            public void AddFactVyr(double addition) {
                this.FactVyr += addition;
            }

            public void AddPlanVyr(double addition) {
                this.PlanVyr += addition;
            }

            public double GetDeltaVyr() {
                return (this.FactVyr - this.PlanVyr);
            }

            public double GetFactVyr() {
                return this.FactVyr;
            }

            public double GetPlanVyr() {
                return this.PlanVyr;
            }

            public void ToNULL() {
                this.PlanVyr = this.PlanVyrToday = this.PlanVyrYesterday = 0.0;
                this.FactVyr = this.FactVyrToday = this.FactVyrYesterday = 0.0;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) {
            int si = tabControl1.SelectedIndex;
            switch (si) {
                case 1:
                    zg0.AxisChange();
                    zg0.Invalidate();
                    break;
                case 2:
                    zg1.AxisChange();
                    zg1.Invalidate();
                    break;
                case 3:
                    zg2.AxisChange();
                    zg2.Invalidate();
                    break;
                case 4:
                    zg3.AxisChange();
                    zg3.Invalidate();
                    break;
                case 5:
                    zg4.AxisChange();
                    zg4.Invalidate();
                    break;
                case 6:
                    zg5.AxisChange();
                    zg5.Invalidate();
                    break;
                case 7:
                    zg6.AxisChange();
                    zg6.Invalidate();
                    break;
                case 8:
                    zg7.AxisChange();
                    zg7.Invalidate();
                    break;
                case 9:
                    zg8.AxisChange();
                    zg8.Invalidate();
                    break;
                case 10:
                    zg9.AxisChange();
                    zg9.Invalidate();
                    break;
                case 11:
                    zg10.AxisChange();
                    zg10.Invalidate();
                    break;
            }
        }
    }
}
