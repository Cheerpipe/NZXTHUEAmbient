using System.Drawing;
using System;
using System.Threading;
using System.Diagnostics;
using System.Linq;

namespace NZXTHUEAmbient
{
    public class Program
    {
        //private static HUE2AmbientDevice controller = new HUE2AmbientDevice();
        static void Main(string[] args)
        {
            /*
            My monitor Setup 10 x 18 

                10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27
            9                                                       28
            8                                                       29                                                     
            7                                                       30
 =          6                                                       31
            5                                                       32
            4                                                       33
            3                                                       34
            2                                                       35
            1                                                       36 
            0                                                       37
 18           55 54 53 52 51 50 49 48 47 46 45 44 43 42 41 40 39 38 
            */

            /*
            My desktop Setup 10 x 18 

            25                                                                                                  26
            24                                                                                                   27
            23                                                                                                   28
            22                                                                                                   29
            21                                                                                                   30
            20                                                                                                   31
            19                                                                                                   32
            18                                                                                                   33
            17                                                                                                   34
            16                                                                                                   35
16            15 14 13 12 11 10 9 8 7 6 5 4 3 2 1 0 [CONTROLLER] 51 50 49 48 47 46 45 44 43 42 41 40 39 38 37 36
             */
            HUE2AmbientDeviceLoader.InitDevices().Wait();

            foreach (HUE2AmbientDeviceController controller in HUE2AmbientDeviceLoader.Devices)

            {
                int totalLeds = controller.TotalLedCount;

                AllExample(Color.FromArgb(255, 0, 0), controller);
                Thread.Sleep(500);
                AllExample(Color.FromArgb(0, 255, 0), controller);
                Thread.Sleep(500);
                AllExample(Color.FromArgb(0, 0, 255), controller);
                Thread.Sleep(500);
                AllExample(Color.FromArgb(0, 255, 255), controller);
                Thread.Sleep(500);
                AllExample(Color.FromArgb(255, 0, 255), controller);
                Thread.Sleep(500);
                AllExample(Color.FromArgb(255, 255, 0), controller);
                Thread.Sleep(500);
                AllExample(Color.FromArgb(255, 255, 255), controller);
                Thread.Sleep(500);
                AllExample(Color.FromArgb(0, 0, 0), controller);
                /*
                                TransactionExample1(totalLeds);
                                Thread.Sleep(500);
                                AllExample(Color.FromArgb(255, 255, 255));
                                TransactionExample2(totalLeds);
                                Thread.Sleep(500);
                                AllExample(Color.FromArgb(0, 0, 0));
                                TransactionExample3(totalLeds);
                                Thread.Sleep(500);
                 */
                LoopExample(totalLeds, controller);

            }





        }

        private static void AllExample(Color color, HUE2AmbientDeviceController controller)
        {
            controller.SetLedsSync(color);
        }


        private static void TransactionExample1(int totalLeds)
        {
            Color corner1 = Color.FromArgb(255, 0, 0);
            Color corner2 = Color.FromArgb(0, 0255, 0);
            Color corner3 = Color.FromArgb(0, 0, 255);
            Color corner4 = Color.FromArgb(255, 0255, 255);
            HUE2AmbientDeviceLoader.Devices[1].TransactionStart(1000);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(0, corner1);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(55, corner1);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(9, corner2);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(10, corner2);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(27, corner3);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(28, corner3);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(37, corner4);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(38, corner4);
            HUE2AmbientDeviceLoader.Devices[1].TransactionCommit();
        }

        private static void TransactionExample2(int totalLeds)
        {
            Color top = Color.FromArgb(255, 0, 0);
            Color bottom = Color.FromArgb(0, 0, 255);
            HUE2AmbientDeviceLoader.Devices[1].TransactionStart();
            for (byte i = 10; i <= 27; i++)
                HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(i, top);

            for (byte i = 38; i <= 55; i++)
                HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(i, bottom);
            HUE2AmbientDeviceLoader.Devices[1].TransactionCommit();
        }


        private static void TransactionExample3(int totalLeds)
        {
            Color corner1 = Color.FromArgb(255, 0, 0);
            Color corner2 = Color.FromArgb(0, 0255, 0);
            Color corner3 = Color.FromArgb(0, 0, 255);
            Color corner4 = Color.FromArgb(255, 0, 255);
            HUE2AmbientDeviceLoader.Devices[1].TransactionStart();
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(0, corner1);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(55, corner1);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(9, corner2);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(10, corner2);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(27, corner3);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(28, corner3);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(37, corner4);
            HUE2AmbientDeviceLoader.Devices[1].TransactionSetLed(38, corner4);
            HUE2AmbientDeviceLoader.Devices[1].TransactionCommit();
        }


        private static void LoopExample(int totalLeds, HUE2AmbientDeviceController controller)
        {
            for (int j = 0; j <= 5; j++)
            {
                Color[] newColors = new Color[totalLeds];
                for (int i = 0; i < newColors.Length; i++)
                {
                    newColors[i] = Color.FromArgb(255, 0, 0);
                    if (i == 0)
                        newColors[totalLeds - 1] = Color.FromArgb(0, 0, 0);
                    else
                        newColors[i - 1] = Color.FromArgb(0, 0, 0);
                    controller.SetLeds(newColors, HUE2AmbientDeviceController.LayoutType.Linear).Wait();


                }
            }
        }
    }
}
