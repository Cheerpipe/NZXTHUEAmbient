using System;
using System.IO;
using System.IO.Pipes;


namespace NZXTHUEAmbientSetter
{
    public class ArgsPipeInterOp
    {
        public static bool StopListening = false;
        public void StartArgsPipeServer()
        {
            var s = new NamedPipeServerStream("NZXTHUEAmbientSetter", PipeDirection.In);
            Action<NamedPipeServerStream> a = GetArgsCallBack;
            a.BeginInvoke(s, callback: ar => { }, @object: null);
        }

        private static void GetArgsCallBack(NamedPipeServerStream pipe)
        {
            while (!StopListening)
            {
                pipe.WaitForConnection();
                var sr = new StreamReader(pipe);
                var args = sr.ReadToEnd().Split(' ');
                Program.Run(args);
                pipe.Disconnect();
            }
        }
    }
}
