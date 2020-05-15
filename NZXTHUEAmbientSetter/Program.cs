
using Microsoft.Win32;
using NZXTHUEAmbient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NZXTHUEAmbientSetter
{
    // For use with Aurora Project
    // https://github.com/antonpup/Aurora or any other program using NamedPipes

    static class Program
    {
        private static HUE2AmbientController controller = new HUE2AmbientController();
        private static Thread _setterThread;
        private static readonly ManualResetEvent _setterThreadEvent = new ManualResetEvent(false);
        private static byte R = 0;
        private static byte G = 0;
        private static byte B = 0;

        static void Main()
        {
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            controller.InitDeviceSync(56); //I have 56 leds
            var pipeInterOp = new ArgsPipeInterOp();

            _setterThread = new Thread(DoSetter);
            _setterThread.SetApartmentState(ApartmentState.STA);
            _setterThread.Start();
            pipeInterOp.StartArgsPipeServer();
            _setterThread.Join();

        }

        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            Run(new string[] { "shutdown" });
        }

        public static void DoSetter()
        {
            while (_setterThread.IsAlive)
            {
                _setterThreadEvent.WaitOne();
                controller.SetLedsSync(Color.FromArgb(R, G, B));
                _setterThreadEvent.Reset();
            }
        }

        public static void Run(string[] args)
        {
            if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "shutdown":
                        ArgsPipeInterOp.StopListening = true;
                        R = 0;
                        G = 0;
                        B = 0;
                        _setterThreadEvent.Set();
                        Thread.Sleep(1000);
                        _setterThread.Abort();
                        break;
                }
            }
            else if (args.Length == 3)
            {
                R = Convert.ToByte(args[0]);
                G = Convert.ToByte(args[1]);
                B = Convert.ToByte(args[2]);
                _setterThreadEvent.Set();
            }
            else
            {
                //do nothing
            }
        }
    }
}
