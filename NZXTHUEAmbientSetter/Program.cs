using NZXTHUEAmbient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NZXTHUEAmbientSetter
{
    static class Program
    {
        private static NZXTHUEAmbientDevice _device;
        private static Thread _setterThread;
        private static readonly ManualResetEvent _setterThreadEvent = new ManualResetEvent(false);
        private static byte R = 0;
        private static byte G = 0;
        private static byte B = 0;

        static void Main()
        {
            _device = new NZXTHUEAmbientDevice();
            _device.RegisterDeviceFactory();
            _device.InitDeviceSync();
            var pipeInterOp = new ArgsPipeInterOp();

            _setterThread = new Thread(DoSetter);
            _setterThread.SetApartmentState(ApartmentState.STA);
            _setterThread.Start();
            pipeInterOp.StartArgsPipeServer();
            _setterThread.Join();

        }

        public static void DoSetter()
        {
            while (_setterThread.IsAlive)
            {
                _setterThreadEvent.WaitOne();
                _device.SetAllLedsSync(R, G, B);
                _setterThreadEvent.Reset();
            }
        }


        public static void Run(string[] args)
        {
            R = Convert.ToByte(args[0]);
            G = Convert.ToByte(args[1]);
            B = Convert.ToByte(args[2]);
            _setterThreadEvent.Set();
        }
    }
}
