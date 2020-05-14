
using System.Threading;

namespace NZXTHUEAmbient
{
    public class Program
    {
        private NZXTHUEAmbientDevice _device;

        static void Main(string[] args)
        {

            NZXTHUEAmbientDevice _device = new NZXTHUEAmbientDevice();
            _device.RegisterDeviceFactory();
            _device.InitDeviceSync();




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



        }

    }
}
