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

public class NZXTHUEAmbientMonitor
{
    public string devicename = "NZXT HUE Ambient Monitor";
    public bool enabled = true; //Switch to True, to enable it in Aurora
    private Color device_color = Color.Black;
	
    public bool Initialize()
    {
        try
        {
			KillProcessByName("NZXT CAM.exe");
			KillProcessByName("NZXTHUEAmbientListener.exe");
			Thread.Sleep(500);
			Process.Start("NZXTHUEAmbientListener.exe");
            return true;
        }
        catch(Exception)
        {
            return false;
        }
    }

	public void SendArgs(byte[] args)
	{
		return;
		using (var pipe = new NamedPipeClientStream(".", "HUE2AmbientDeviceController1", PipeDirection.Out))
		using (var stream = new BinaryWriter(pipe))
		{
			pipe.Connect(timeout: 10);
			stream.Write(args);
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
        Thread.Sleep(1000);		
		Initialize();
    }
    
    public void Shutdown()
    {
		SendArgs(new byte[] { 5,0,0,0,0 });	
		Thread.Sleep(1000);
    }
    
    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
    {
        try
        {
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
				if(key.Key == DeviceKeys.ADDITIONALLIGHT1)
                {
					SendArgs(new byte[] { 3,0,0,0,0 });	
					//Horizontal
					SendColorToDevice(key.Value, true,9);
					SendColorToDevice(key.Value, true,8);
					SendColorToDevice(key.Value, true,7);					
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT2)
                {
					SendColorToDevice(key.Value, true,6);
					SendColorToDevice(key.Value, true,5);
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT3)
                {
					SendColorToDevice(key.Value, true,4);
					SendColorToDevice(key.Value, true,3);
					
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT4)
                {
					SendColorToDevice(key.Value, true,2);
					SendColorToDevice(key.Value, true,1);
					SendColorToDevice(key.Value, true,0);
				}				
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT5)
                {
					SendColorToDevice(key.Value, true,55);
				}	
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT27)
                {					
					SendColorToDevice(key.Value, true,10);
                }
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT6)
				{
					SendColorToDevice(key.Value, true,54);
				}	
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT28)
                {					
					SendColorToDevice(key.Value, true,11);
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT7)
                {
					SendColorToDevice(key.Value, true,53);
				}	
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT29)
                {					
					SendColorToDevice(key.Value, true,12);
                }
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT8)
                {
					SendColorToDevice(key.Value, true,52);
				}	
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT30)
                {					
                    SendColorToDevice(key.Value, true,13);
                }
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT9)
                {
					SendColorToDevice(key.Value, true,51);					
				}	
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT31)
                {					
                    SendColorToDevice(key.Value, true,14);
                }
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT10)
                {
					SendColorToDevice(key.Value, true,50);
				}	
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT32)
                {					
					SendColorToDevice(key.Value, true,15);			
                }
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT11)
                {
					SendColorToDevice(key.Value, true,49);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT1)
                {					
					SendColorToDevice(key.Value, true,16);
                }
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT12)
                {
					SendColorToDevice(key.Value, true,48);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT2)
                {						
					SendColorToDevice(key.Value, true,17);
                }
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT13)
                {
					SendColorToDevice(key.Value, true,47);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT3)
                {						
					SendColorToDevice(key.Value, true,18);			
                }
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT14)
                {
					SendColorToDevice(key.Value, true,46);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT4)
                {						
					SendColorToDevice(key.Value, true,19);
                }				
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT15)
                {
					SendColorToDevice(key.Value, true,45);	
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT5)
                {						
					SendColorToDevice(key.Value, true,20);	
                }				
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT16)
                {
					SendColorToDevice(key.Value, true,44);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT6)
                {						
					SendColorToDevice(key.Value, true,21);
                }	
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT17)
				{
					SendColorToDevice(key.Value, true,43);	
				}
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT7)
                {	
					SendColorToDevice(key.Value, true,22);
				}	
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT18)
				{
					SendColorToDevice(key.Value, true,42);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT8)
                {						
					SendColorToDevice(key.Value, true,23);	
				}					
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT19)
				{
					SendColorToDevice(key.Value, true,41);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT9)
                {	
					SendColorToDevice(key.Value, true,24);
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT20)
				{
					SendColorToDevice(key.Value, true,40);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT10)
                {						
					SendColorToDevice(key.Value, true,25);
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT21)
				{
					SendColorToDevice(key.Value, true,39);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT11)
                {						
					SendColorToDevice(key.Value, true,26);
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT22 )
				{
					SendColorToDevice(key.Value, true,38);
                }
				else if(key.Key == DeviceKeys.MOUSEPADLIGHT12)
                {						
					SendColorToDevice(key.Value, true,27);
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT23)
				{
					SendColorToDevice(key.Value, true,28);	
					SendColorToDevice(key.Value, true,29);
					SendColorToDevice(key.Value, true,30);
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT24)
				{
					SendColorToDevice(key.Value, true,31);	
					SendColorToDevice(key.Value, true,32);
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT25)
				{
					SendColorToDevice(key.Value, true,33);	
					SendColorToDevice(key.Value, true,34);
				}
				else if(key.Key == DeviceKeys.ADDITIONALLIGHT26)
				{
					SendColorToDevice(key.Value, true,35);	
					SendColorToDevice(key.Value, true,36);
					SendColorToDevice(key.Value, true,37);
					SendArgs(new byte[] { 4,0,0,0,0 });	
				}
            }
            return true;
        }
        catch(Exception)
        {
            return false;
        }
    }
    
    //Custom method to send the color to the device
    private void SendColorToDevice(Color color, bool forced, byte led = 0)
    {
       //Check if device's current color is the same, no need to update if they are the same		
	   //0 nothing
	   //1 setledtrx
	   //2 setledall
	   //3 start trx
	   //4 commit trx 
	   //6 shutdown
        if (!device_color.Equals(color) || forced)
        {
			device_color=color;	
//			string command = string.Format("{0} {1} {2} {3}", Convert.ToInt32(color.R*color.A/255).ToString(), Convert.ToInt32(color.G*color.A/255).ToString(), Convert.ToInt32(color.B*color.A/255).ToString(), led > -1 ? led.ToString() : "" );
//			SendArgs(new string[] { command });
			SendArgs(new byte[5] { 1, Convert.ToByte(color.R*color.A/255), Convert.ToByte(color.G*color.A/255), Convert.ToByte(color.B*color.A/255), led });

        }
	}
}