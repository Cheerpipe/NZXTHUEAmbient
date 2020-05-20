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
                var sr = new BinaryReader(pipe);
                var args = sr.ReadBytes(5);
                Setter(args);
                pipe.Disconnect();
            }
        }


        public void SendArgs(byte[] args)
        {
            using (var pipe = new NamedPipeClientStream(".", "HUE2AmbientDeviceController1", PipeDirection.Out))
            using (var stream = new BinaryWriter(pipe))
            {
                pipe.Connect(timeout: 15);
                stream.Write(args);
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


        public void Setter(byte[] args)
        {
            if (_shutingDown)
                return;
            /*
            Command structure:
            0: command id
            1: R
            2: G
            3: B
            4: led index

            //Command id
            //0 nothing
            //1 setledtrx
            //2 setledall
            //3 start trx
            //4 commit trx 
            //5 shutdown
            */
            if (args[0] == 1)
            {
                R = Convert.ToByte(args[1]);
                G = Convert.ToByte(args[2]);
                B = Convert.ToByte(args[3]);
                byte led = Convert.ToByte(args[4]);
                _deviceController.TransactionSetLed(led, Color.FromArgb(R, G, B));
            }
            else if (args[0] == 2)
            {
                R = Convert.ToByte(args[1]);
                G = Convert.ToByte(args[2]);
                B = Convert.ToByte(args[3]);
                _setterThreadEvent.Set();
            }
            else if (args[0] == 3)
            {
                _deviceController.TransactionStart(1000);
            }
            else if (args[0] == 4)
            {
                _deviceController.TransactionCommit();
            }
            else if (args[0] == 5)
            {
                _shutingDown = true;
                // _.StopListening = true;
                R = 0;
                G = 0;
                B = 0;
                _setterThreadEvent.Set();
                Thread.Sleep(1000);
                _setterThread.Abort();
            }
        }
    }
}
