
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
        private static Thread _setterThread;
        private static readonly ManualResetEvent _setterThreadEvent = new ManualResetEvent(false);
        private static HUE2AmbientDeviceController _deviceController;
        private static byte R = 0;
        private static byte G = 0;
        private static byte B = 0;
        private static bool _shutingDown = false;

        static void Main()
        {
            HUE2AmbientDeviceLoader.InitDevices().Wait();
            _deviceController = HUE2AmbientDeviceLoader.Devices.FirstOrDefault();

            if (HUE2AmbientDeviceLoader.Devices.Length == 0)
            {
                throw new Exception("No HUE 2 Ambiente devices found");
            }
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            ArgsPipeInterOp pipeInterOpDevice0 = new ArgsPipeInterOp();

            _setterThread = new Thread(DoSetter);
            _setterThread.SetApartmentState(ApartmentState.STA);
            _setterThread.Start();
            pipeInterOpDevice0.StartArgsPipeServer("NZXTHUEAmbientSetterDevice0");
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
                _deviceController.SetLedsSync(Color.FromArgb(R, G, B));
                _setterThreadEvent.Reset();
            }
        }

        public static void Run(string[] args)
        {
            if (_shutingDown)
                return;
            //TODO: implement propper command parser
            //TODO: allow arrays as command params to reduce pipe call ammount.
            if (args.Length == 4)
            {
                R = Convert.ToByte(args[0]);
                G = Convert.ToByte(args[1]);
                B = Convert.ToByte(args[2]);
                byte led = Convert.ToByte(args[3]);
                _deviceController.TransactionSetLed(led, Color.FromArgb(R, G, B));
            }
            else if (args.Length == 3)
            {
                R = Convert.ToByte(args[0]);
                G = Convert.ToByte(args[1]);
                B = Convert.ToByte(args[2]);
                _setterThreadEvent.Set();
            }
            else if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "transactionstart":
                        _deviceController.TransactionStart(1000);
                        break;
                    case "transactioncommit":
                        _deviceController.TransactionCommit();
                        break;
                    case "shutdown":
                        _shutingDown = true;
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
        }
    }
}
