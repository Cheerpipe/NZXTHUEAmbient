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
            controller.InitDeviceSync();
           /*
            controller.SetLedsSync(Color.FromArgb(255, 0, 0));
            Thread.Sleep(500);
            controller.SetLedsSync(Color.FromArgb(0, 255, 0));
            Thread.Sleep(500);
            controller.SetLedsSync(Color.FromArgb(0, 0, 255));
            Thread.Sleep(500);
          */
         
            do
            {
                Color[] newColors = new Color[60];
                for (int i = 0; i < 60; i++)
                {
                    newColors[i] = Color.FromArgb(255, 255, 255);
                    if (i == 0)
                        newColors[49] = Color.FromArgb(255, 255, 255);
                    else
                        newColors[i - 1] = Color.FromArgb(0, 0, 0);
                    controller.SetLedsSync(newColors);
                    Thread.Sleep(16);
                }

            } while (true);
          
            /*

            _device.SetAllLedsSync(0, 0, 255);
            Thread.Sleep(100);
            _device.SetAllLedsSync(0, 255, 0);
            Thread.Sleep(100);
            _device.SetAllLedsSync(255, 0, 0);
            Thread.Sleep(100);
            _device.SetAllLedsSync(255, 0, 255);
            Thread.Sleep(100);
            _device.SetAllLedsSync(255, 255, 0);
            Thread.Sleep(100);
            _device.SetAllLedsSync(0, 255, 255);
            Thread.Sleep(100);
            */


        }

    }
}
