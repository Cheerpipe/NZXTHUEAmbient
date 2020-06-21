
using Microsoft.Win32;
using NZXTHUEAmbient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace NZXTHUEAmbientListener
{
    // For use with Aurora Project Scripted Device or any other program using NamedPipes
    // https://github.com/antonpup/Aurora 

    internal static class Program
    {
        internal static Mutex _singleInstanceMutex;
        static private int devIndex;
        private static List<Listener> _listeners = new List<Listener>();
        static void Main(string[] args)
        {
            if (args.Contains("--ngen"))
            {
                Util.ngen(Util.NgenOperation.Install);
                return;
            }
            if (args.Contains("--unngen"))
            {
                Util.ngen(Util.NgenOperation.Uninstall);
                return;
            }

            bool useLastSetting = args.Contains("--uselastsetting");

            try
            {
                int.TryParse(args.Where(a => a.Contains("--dev:")).FirstOrDefault().Split(':')[1], out devIndex);
            }
            catch
            {
                throw new Exception("No device index supplied");
            }
            _singleInstanceMutex = new Mutex(true, "{7073d39a-532d-4b06-bde3-19732814ee77}-" + devIndex);

            //No more than one instance runing
            if (!_singleInstanceMutex.WaitOne(TimeSpan.Zero, true))
                return;

            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            SystemEvents.SessionEnded += SystemEvents_SessionEnded;

            HUE2AmbientDeviceLoader.InitDevice(devIndex, useLastSetting).Wait();

            if (HUE2AmbientDeviceLoader.Devices.Length == 0)
            {
                throw new Exception("No HUE 2 Ambiente devices found");
            }

            Listener _listener = new Listener(HUE2AmbientDeviceLoader.Devices[devIndex], devIndex);
            _listeners.Add(_listener);
            Application.Run();
        }

        private static void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            foreach (Listener l in _listeners)
            {
                l.Setter(new byte[] { 1, 5, 0, 0, 0, 0 });
            }
        }

        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            foreach (Listener l in _listeners)
            {
                l.Setter(new byte[] { 1, 5, 0, 0, 0, 0 });
            }
        }
    }
}
