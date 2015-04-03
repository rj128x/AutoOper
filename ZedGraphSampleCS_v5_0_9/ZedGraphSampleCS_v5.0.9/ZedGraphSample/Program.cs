using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Collections;

namespace ZedGraphSample
{
    public class Car
    {
        public int CurrSpeed;
        public int MaxSpeed;
        public string name;
        public bool dead;
        public delegate void EngineHandler(string msg);
        public event EngineHandler Exploded;
        public event EngineHandler AboutToBlow;

        public Car(string n, int ms, int cs)
        {
            this.name = n;
            this.MaxSpeed = ms;
            this.CurrSpeed = cs;
        }

        public void SpeedUp(int delta)
        {
            if (dead)
            {
                if (Exploded != null)
                    Exploded("Sorry! This is dead!");
            }
            else
            {
                CurrSpeed += delta;
            }

            if (MaxSpeed - CurrSpeed <= 10)
                if (AboutToBlow != null)
                    AboutToBlow("Be careful!!!Approaching speed limit!");

            if (CurrSpeed > MaxSpeed)
                dead = true;
            else
                Console.WriteLine("\t Currspeed = {0}", CurrSpeed);


        }
    }





    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]



        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Console.WriteLine("hello!");
            Car car1 = new Car("Ford", 100, 10);
            car1.Exploded += new Car.EngineHandler(OnBlowUp);
            car1.AboutToBlow += new Car.EngineHandler(OnAboutToBlow);

            for (int i = 0; i < 10; i++)
                car1.SpeedUp(20);
        }

        public static void OnBlowUp(string s)
        {
            Console.WriteLine("Message from car: {0} ",s);        
        }

        public static void OnAboutToBlow(string s)
        {
            Console.WriteLine("Message from car: {0} ", s);
        h}
    }

}