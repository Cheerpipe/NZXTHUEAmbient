using System.Drawing;
using System;
using System.Threading;

namespace NZXTHUEAmbient
{
    public class Program
    {
        private static HUE2AmbientController controller = new HUE2AmbientController();
        static void Main(string[] args)
        {
            int totalLeds = 56; //28 per channel
            controller.InitDeviceSync(56);
           

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
            Thread.Sleep(3000);
            AllExample(Color.FromArgb(0, 0, 0));
            TransactionExample(totalLeds); //Corners only
            Thread.Sleep(3000);
            LoopExample(totalLeds);
        }

        private static void AllExample(Color color)
        {
            controller.SetLedsSync(color);
        }


        private static void TransactionExample(int totalLeds)
        {
            Color corner1 = Color.FromArgb(255, 0, 0);
            Color corner2 = Color.FromArgb(0, 0255, 0);
            Color corner3 = Color.FromArgb(0, 0, 255);
            Color corner4 = Color.FromArgb(255, 0255, 255);
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
            Thread.Sleep(3000);
            controller.TransactionStart();
            controller.TransactionSetLed(0, corner4);
            controller.TransactionSetLed(55, corner4);
            controller.TransactionSetLed(9, corner4);
            controller.TransactionSetLed(10, corner4);
            controller.TransactionSetLed(27, corner4);
            controller.TransactionSetLed(28, corner4);
            controller.TransactionSetLed(37, corner4);
            controller.TransactionSetLed(38, corner4);
            controller.TransactionCommit();
        }

        private static void LoopExample(int totalLeds)
        {
            for (int j = 0; j <=  3; j++) // 3 loops
            {
                Color[] newColors = new Color[totalLeds];
                for (int i = 0; i < newColors.Length; i++)
                {
                    newColors[i] = Color.FromArgb(255, 255, 255);
                    if (i == 0)
                        newColors[55] = Color.FromArgb(0, 0, 0);
                    else
                        newColors[i - 1] = Color.FromArgb(0, 0, 0);
                    controller.SetLedsSync(newColors);
                    Thread.Sleep(16);
                }

            }
        }
    }
}
