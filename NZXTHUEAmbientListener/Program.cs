
using Microsoft.Win32;
using NZXTHUEAmbient;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NZXTHUEAmbientListener
{
    // For use with Aurora Project Scripted Device or any other program using NamedPipes
    // https://github.com/antonpup/Aurora 

    static class Program
    {
        private static List<Listener> _listeners = new List<Listener>();
        static void Main()
        {
            HUE2AmbientDeviceLoader.InitDevices().Wait();

            if (HUE2AmbientDeviceLoader.Devices.Length == 0)
            {
                throw new Exception("No HUE 2 Ambiente devices found");
            }

            SystemEvents.SessionEnding += SystemEvents_SessionEnding;

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
                l.Setter(new byte[] { 5, 0, 0, 0, 0 });
            }
        }
    }
}
