﻿using NZXTHUEAmbient;
using System;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;

namespace NZXTHUEAmbientListener
{
    public class Listener
    {
        public bool StopListening = false;
        //private Thread _setterThread;
        //private readonly ManualResetEvent _setterThreadEvent = new ManualResetEvent(false);
        private bool _shutingDown = false;
        private HUE2AmbientDeviceController _deviceController;
        private byte R = 0;
        private byte G = 0;
        private byte B = 0;

        public Listener(HUE2AmbientDeviceController hue2AmbientDeviceController, int _deviceId)
        {
            _deviceController = hue2AmbientDeviceController;
            //_setterThread = new Thread(DoSetter);
            //_setterThread.Start();
            StartArgsPipeServer("HUE2AmbientDeviceController" + _deviceId.ToString());
        }

        //TODO: Use MMF instead of pipes
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
                var args = sr.ReadBytes(512);
                pipe.Disconnect();
                Setter(args);
            }
        }
        /*
                public void DoSetter()
                {
                    while (_setterThread.IsAlive)
                    {
                        _setterThreadEvent.WaitOne();
                        _deviceController.SetLedsSync(Color.FromArgb(R, G, B));
                        _setterThreadEvent.Reset();
                    }
                }
        */
        public void Setter(byte[] args)
        {
            if (_shutingDown)
                return;
            /*
            
            First byte is the command count. Total packet lenght will by command count * command leng 

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

            int commandCount = args[0];

            //For each command in data packet process command.
            for (int i = 0; i < commandCount; i++)
            {
                if (args[i * 5 + 1] == 1) //setledtrx
                {
                    R = args[i * 5 + 2];
                    G = args[i * 5 + 3];
                    B = args[i * 5 + 4];
                    byte led = args[i * 5 + 5];
                    _deviceController.SetLed(led, Color.FromArgb(R, G, B));
                }
                else if (args[i * 5 + 1] == 2) // Setleds (All)
                {
                    R = args[i * 5 + 2];
                    G = args[i * 5 + 3];
                    B = args[i * 5 + 4];
                    _deviceController.SetLeds(Color.FromArgb(R, G, B));
                }
                else if (args[i * 5 + 1] == 4) // Commit
                {
                    _deviceController.Apply();
                }
                else if (args[i * 5 + 1] == 2)
                {
                    R = args[i * 5 + 2];
                    G = args[i * 5 + 3];
                    B = args[i * 5 + 4];
                    //_setterThreadEvent.Set();
                }
                else if (args[i * 5 + 1] == 5) //Shutdown
                {
                    _shutingDown = true;

                    _deviceController.SetLeds(Color.FromArgb(0, 0, 0));
                    _deviceController.SetLeds(Color.FromArgb(0, 0, 0));
                    _deviceController.SetLeds(Color.FromArgb(0, 0, 0));
                    _deviceController.Dispose();
                    StopListening = true;
                    Application.Exit();
                }
                else if (args[i * 5 + 1] == 6) //Reset
                {
                    _deviceController.ReInitialize();
                }
            }
        }
    }
}
