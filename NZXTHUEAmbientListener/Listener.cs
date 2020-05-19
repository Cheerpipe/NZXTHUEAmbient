using NZXTHUEAmbient;
using System;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace NZXTHUEAmbientListener
{
    public class Listener
    {
        public bool StopListening = false;
        private Thread _setterThread;
        private readonly ManualResetEvent _setterThreadEvent = new ManualResetEvent(false);
        private bool _shutingDown = false;
        private HUE2AmbientDeviceController _deviceController;
        private byte R = 0;
        private byte G = 0;
        private byte B = 0;

        public Listener(HUE2AmbientDeviceController hue2AmbientDeviceController, int _deviceId)
        {
            _deviceController = hue2AmbientDeviceController;
            _setterThread = new Thread(DoSetter);
            _setterThread.Start();
            StartArgsPipeServer("HUE2AmbientDeviceController" + _deviceId.ToString());
            _setterThread.Join();
        }

        public void StartArgsPipeServer(string pipeName)
        {
            var s = new NamedPipeServerStream(pipeName, PipeDirection.In);
            Action<NamedPipeServerStream> a = GetArgsCallBack;
            a.BeginInvoke(s, callback: ar => { }, @object: null);
        }

        private void GetArgsCallBack(NamedPipeServerStream pipe)
        {
            while (!StopListening)
            {
                pipe.WaitForConnection();
                var sr = new StreamReader(pipe);
                var args = sr.ReadToEnd().Split(' ');
                Setter(args);
                pipe.Disconnect();
            }
        }

        public void DoSetter()
        {
            while (_setterThread.IsAlive)
            {
                _setterThreadEvent.WaitOne();
                _deviceController.SetLedsSync(Color.FromArgb(R, G, B));
                _setterThreadEvent.Reset();
            }
        }


        public void Setter(string[] args)
        {
            if (_shutingDown)
                return;

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
                        // _.StopListening = true;
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
