using Aurora;
using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

public class NZXTHUEAmbient
{
    public string devicename = "NZXT HUE Ambient Monitor";
    public bool enabled = true; //Switch to True, to enable it in Aurora
    private Color device_color = Color.Black;
	
    public bool Initialize()
    {
        try
        {
			KillProcessByName("NZXT CAM.exe");
			KillProcessByName("NZXTHUEAmbientSetter.exe");
			Thread.Sleep(100);
			Process.Start(@"D:\Warez\Utiles\NZXTHUEAmbientSetter\NZXTHUEAmbientSetter.exe");
            return true;
        }
        catch(Exception exc)
        {
            return false;
        }
    }
  
	public void SendArgs(string[] args)
	{
		using (var pipe = new NamedPipeClientStream(".", "NZXTHUEAmbientSetterDevice0", PipeDirection.Out))
		using (var stream = new StreamWriter(pipe))
		{
			pipe.Connect(timeout: 100);
			stream.Write(string.Join(separator: " ", value: args));
		}
	}	
	
	public static void KillProcessByName(string processName)
	{
		Process cmd = new Process();
		cmd.StartInfo.FileName = @"C:\Windows\System32\taskkill.exe";
		cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		cmd.StartInfo.Arguments = string.Format(@"/f /im {0}", processName);
		cmd.Start();
		cmd.Dispose();
	}	
    
    public void Reset()
    {
		Shutdown();
		Initialize();
    }
    
    public void Shutdown()
    {
		//SendColorToDevice(Color.FromArgb(0,0,0), true); //Should not be necessary
		SendArgs(new string[] { "shutdown" });
    }
    
    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
    {
        try
        {
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
				if(key.Key == DeviceKeys.F1)
                {
                    SendArgs(new string[] { "transactionstart" });
					
					SendColorToDevice(key.Value, forced,0);
					SendColorToDevice(key.Value, true,1);
					SendColorToDevice(key.Value, true,2);
					SendColorToDevice(key.Value, true,3);
					SendColorToDevice(key.Value, true,4);
					SendColorToDevice(key.Value, true,5);
					SendColorToDevice(key.Value, true,6);
					SendColorToDevice(key.Value, true,7);
					SendColorToDevice(key.Value, true,8);
					SendColorToDevice(key.Value, true,9);
					SendColorToDevice(key.Value, true,10);
					SendColorToDevice(key.Value, true,11);
					
					SendColorToDevice(key.Value, true,55);
					SendColorToDevice(key.Value, true,54);					
					
                }
				if(key.Key == DeviceKeys.F2)
                {
					SendColorToDevice(key.Value, forced,12);					
					SendColorToDevice(key.Value, true,13);
					SendColorToDevice(key.Value, true,53);
					SendColorToDevice(key.Value, true,52);										
                }
				if(key.Key == DeviceKeys.F3)
                {
                    SendColorToDevice(key.Value, forced,14);
					SendColorToDevice(key.Value, true,15);
					SendColorToDevice(key.Value, true,51);
					SendColorToDevice(key.Value, true,50);
                }
				if(key.Key == DeviceKeys.F4)
                {
                    SendColorToDevice(key.Value, forced,16);
					SendColorToDevice(key.Value, true,17);
					SendColorToDevice(key.Value, true,49);
					SendColorToDevice(key.Value, true,48);					
                }
				if(key.Key == DeviceKeys.F5)
                {
					SendColorToDevice(key.Value, forced,18);
					SendColorToDevice(key.Value, true,19);
					SendColorToDevice(key.Value, true,47);
					SendColorToDevice(key.Value, true,46);					
                }
				if(key.Key == DeviceKeys.F6)
                {
					SendColorToDevice(key.Value, forced,20);
					SendColorToDevice(key.Value, true,21);
					SendColorToDevice(key.Value, true,45);
					SendColorToDevice(key.Value, true,44);
					
                }
				if(key.Key == DeviceKeys.F7)
                {
					SendColorToDevice(key.Value, forced,22);
					SendColorToDevice(key.Value, true,23);
					SendColorToDevice(key.Value, true,43);
					SendColorToDevice(key.Value, true,42);					
                }
				if(key.Key == DeviceKeys.F8)
                {
					SendColorToDevice(key.Value, forced,24);
					SendColorToDevice(key.Value, true,25);
					SendColorToDevice(key.Value, true,41);
					SendColorToDevice(key.Value, true,40);					
                }
				if(key.Key == DeviceKeys.F9)
                {
					SendColorToDevice(key.Value, forced,26);
					SendColorToDevice(key.Value, true,27);
					
					SendColorToDevice(key.Value, true,28);
					SendColorToDevice(key.Value, true,29);
					SendColorToDevice(key.Value, true,30);
					SendColorToDevice(key.Value, true,31);
					SendColorToDevice(key.Value, true,32);
					SendColorToDevice(key.Value, true,33);
					SendColorToDevice(key.Value, true,34);
					SendColorToDevice(key.Value, true,35);
					SendColorToDevice(key.Value, true,37);
					SendColorToDevice(key.Value, true,37);	
					SendColorToDevice(key.Value, true,39);
					SendColorToDevice(key.Value, true,38);					
					
					SendArgs(new string[] { "transactioncommit" });
                }
            }
            return true;
        }
        catch(Exception exc)
        {
            return false;
        }
    }
    
    //Custom method to send the color to the device
    private void SendColorToDevice(Color color, bool forced, int led = -1)
    {
       //Check if device's current color is the same, no need to update if they are the same		
        if (!device_color.Equals(color) || forced)
        {
			device_color=color;	
			string command = string.Format("{0} {1} {2} {3}", Convert.ToInt32(color.R*color.A/255).ToString(), Convert.ToInt32(color.G*color.A/255).ToString(), Convert.ToInt32(color.B*color.A/255).ToString(), led > -1 ? led.ToString() : "" );
			SendArgs(new string[] { command });
        }
	}
}