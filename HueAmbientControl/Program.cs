using System.Drawing;
using System;
using System.Threading;
using System.Diagnostics;

namespace NZXTHUEAmbient
{
    public class Program
    {
        private static HUE2AmbientController controller = new HUE2AmbientController();
        static void Main(string[] args)
        {
            int totalLeds = 56; //28 per channel
            controller.InitDeviceSync(56);


            var stopwatch = new Stopwatch();
            stopwatch.Start();

            controller.TransactionStart(1000);
            for (byte i = 0; i < totalLeds; i++)
            {
                controller.TransactionSetLed(i, Color.FromArgb(255, 255, 255));
            }
            controller.TransactionCommit();


            /*
            My Setup 10 x 18 
            
                10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27
            9                                                       28
            8                                                       29                                                     
            7                                                       30
            6                                                       31
            5                                                       32
            4                                                       33
            3                                                       34
            2                                                       35
            1                                                       36 
            0                                                       37
                55 54 53 52 51 50 49 48 47 46 45 44 43 42 41 40 39 38 
            */


            AllExample(Color.FromArgb(255, 0, 0));
            Thread.Sleep(500);
            AllExample(Color.FromArgb(0, 255, 0));
            Thread.Sleep(500);
            AllExample(Color.FromArgb(0, 0, 255));
            Thread.Sleep(500);
            AllExample(Color.FromArgb(0, 255, 255));
            Thread.Sleep(500);
            AllExample(Color.FromArgb(255, 0, 255));
            Thread.Sleep(500);
            AllExample(Color.FromArgb(255, 255, 0));
            Thread.Sleep(500);
            AllExample(Color.FromArgb(255, 255, 255));
            Thread.Sleep(500);
            AllExample(Color.FromArgb(0, 0, 0));
            TransactionExample1(totalLeds);
            Thread.Sleep(500);
            AllExample(Color.FromArgb(255, 255, 255));
            TransactionExample2(totalLeds);
            Thread.Sleep(500);
            AllExample(Color.FromArgb(0, 0, 0));
            TransactionExample3(totalLeds);
            Thread.Sleep(500);
            LoopExample(totalLeds);
            stopwatch.Stop();
            long elapsed_time = stopwatch.ElapsedMilliseconds;
        }

        private static void AllExample(Color color)
        {
            controller.SetLedsSync(color);
        }


        private static void TransactionExample1(int totalLeds)
        {
            Color corner1 = Color.FromArgb(255, 0, 0);
            Color corner2 = Color.FromArgb(0, 0255, 0);
            Color corner3 = Color.FromArgb(0, 0, 255);
            Color corner4 = Color.FromArgb(255, 0255, 255);
            controller.TransactionStart(1000);
            controller.TransactionSetLed(0, corner1);
            controller.TransactionSetLed(55, corner1);
            controller.TransactionSetLed(9, corner2);
            controller.TransactionSetLed(10, corner2);
            controller.TransactionSetLed(27, corner3);
            controller.TransactionSetLed(28, corner3);
            controller.TransactionSetLed(37, corner4);
            controller.TransactionSetLed(38, corner4);
            controller.TransactionCommit();
        }

        private static void TransactionExample2(int totalLeds)
        {
            Color top = Color.FromArgb(255, 0, 0);
            Color bottom = Color.FromArgb(0, 0, 255);
            controller.TransactionStart();
            for (byte i = 10; i <= 27; i++)
                controller.TransactionSetLed(i, top);

            for (byte i = 38; i <= 55; i++)
                controller.TransactionSetLed(i, bottom);
            controller.TransactionCommit();
        }


        private static void TransactionExample3(int totalLeds)
        {
            Color corner1 = Color.FromArgb(255, 0, 0);
            Color corner2 = Color.FromArgb(0, 0255, 0);
            Color corner3 = Color.FromArgb(0, 0, 255);
            Color corner4 = Color.FromArgb(255, 0, 255);
            controller.TransactionStart();
            controller.TransactionSetLed(0, corner1);
            controller.TransactionSetLed(55, corner1);
            controller.TransactionSetLed(9, corner2);
            controller.TransactionSetLed(10, corner2);
            controller.TransactionSetLed(27, corner3);
            controller.TransactionSetLed(28, corner3);
            controller.TransactionSetLed(37, corner4);
            controller.TransactionSetLed(38, corner4);
            controller.TransactionCommit();
        }


        private static void LoopExample(int totalLeds)
        {
            for (int j = 0; j <= 100; j++)
            {
                Color[] newColors = new Color[totalLeds];
                for (int i = 0; i < newColors.Length; i++)
                {
                    newColors[i] = Color.FromArgb(255, 0, 0);
                    if (i == 0)
                        newColors[55] = Color.FromArgb(0, 0, 0);
                    else
                        newColors[i - 1] = Color.FromArgb(0, 0, 0);
                    controller.SetLeds(newColors).Wait();


                }
            }
        }
    }
}
