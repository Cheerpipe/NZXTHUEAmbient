
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
        private static byte R = 0;
        private static byte G = 0;
        private static byte B = 0;
        private static bool _shutingDown = false;

        static void Main()
        {
            HUE2AmbientDeviceLoader.InitDevices().Wait();
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
                HUE2AmbientDeviceLoader.Devices.FirstOrDefault().SetLedsSync(Color.FromArgb(R, G, B));
                _setterThreadEvent.Reset();
            }
        }

        public static void Run(string[] args)
        {
            if (_shutingDown)
                return;
            //TODO: implement propper command parser
            if (args.Length == 1)
            {
                switch (args[0])
                {
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
                    case "transactionstart":
                        HUE2AmbientDeviceLoader.Devices.FirstOrDefault().TransactionStart(1000);
                        Debug.WriteLine("TRX start");
                        break;
                    case "transactioncommit":
                        HUE2AmbientDeviceLoader.Devices.FirstOrDefault().TransactionCommit();
                        Debug.WriteLine("TRX commit");
                        break;
                }
            }
            else if (args.Length == 3)
            {
                R = Convert.ToByte(args[0]);
                G = Convert.ToByte(args[1]);
                B = Convert.ToByte(args[2]);
                _setterThreadEvent.Set();
                Debug.WriteLine("Setled");
            }
            else if (args.Length == 4)
            {
                Debug.WriteLine("Args:  {0}", args.ToString());
                R = Convert.ToByte(args[0]);
                G = Convert.ToByte(args[1]);
                B = Convert.ToByte(args[2]);
                byte led = Convert.ToByte(args[3]);
                HUE2AmbientDeviceLoader.Devices.FirstOrDefault().TransactionSetLed(led, Color.FromArgb(R, G, B));
                Debug.WriteLine("TRX on led {0}: {1} {2} {3}", led, R, G, B);
            }
            else
            {
                //do nothing
            }
        }
    }
}
