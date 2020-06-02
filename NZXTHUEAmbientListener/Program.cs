
using Microsoft.Win32;
using NZXTHUEAmbient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NZXTHUEAmbientListener
{
    // For use with Aurora Project Scripted Device or any other program using NamedPipes
    // https://github.com/antonpup/Aurora 

    internal static class Program
    {
        internal static Mutex _singleInstanceMutex = new Mutex(true, "{7073d39a-532d-4b06-bde3-19732814ee77}");

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

            //No more than one instance runing
            if (!_singleInstanceMutex.WaitOne(TimeSpan.Zero, true))
                return;

            Util.SetPriorityProcessAndThreads(Process.GetCurrentProcess().ProcessName, ProcessPriorityClass.Idle, ThreadPriorityLevel.Lowest);

            SystemEvents.SessionEnding += SystemEvents_SessionEnding;

            HUE2AmbientDeviceLoader.InitDevices().Wait();

            if (HUE2AmbientDeviceLoader.Devices.Length == 0)
            {
                throw new Exception("No HUE 2 Ambiente devices found");
            }

            for (int c = 0; c < HUE2AmbientDeviceLoader.Devices.Length; c++)
            {
                Listener _listener = new Listener(HUE2AmbientDeviceLoader.Devices[c], c);
                _listeners.Add(_listener); // For issue shutting down or any other broadcast messages
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
