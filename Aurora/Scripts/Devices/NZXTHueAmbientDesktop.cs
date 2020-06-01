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
using CSScriptLibrary;

public class NZXTHUEAmbientDesktop
{
    public string devicename = "NZXT HUE Ambient Desktop";
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
        catch (Exception)
        {
            return false;
        }
    }

    public void SendArgs(byte[] args)
    {
        using (var pipe = new NamedPipeClientStream(".", "HUE2AmbientDeviceController0", PipeDirection.Out))
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
        SendArgs(new byte[] { 5, 0, 0, 0, 0 });
        Thread.Sleep(1000);
    }

    private struct DeviceMapState
    {
        public byte led;
        public Color color;
        public DeviceKeys deviceKey;
        public DeviceMapState(byte led, Color color, DeviceKeys deviceKeys)
        {
            this.led = led;
            this.color = color;
            this.deviceKey = deviceKeys;
        }
    }

    private List<DeviceMapState> deviceMap = new List<DeviceMapState>
    {
      new DeviceMapState(0, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT13),
      new DeviceMapState(1, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT13),
      new DeviceMapState(2, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT13),
      new DeviceMapState(3, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT12),
      new DeviceMapState(4, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT12),
      new DeviceMapState(5, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT12),
      new DeviceMapState(6, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT11),
      new DeviceMapState(7, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT11),
      new DeviceMapState(8, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT11),
      new DeviceMapState(9, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT10),
      new DeviceMapState(10, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT10),
      new DeviceMapState(11, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT10),
      new DeviceMapState(12, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT9),
      new DeviceMapState(13, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT9),
      new DeviceMapState(14, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT9),
      new DeviceMapState(15, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT8),
      new DeviceMapState(16, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT8),
      new DeviceMapState(17, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT8),
      new DeviceMapState(18, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT7),
      new DeviceMapState(19, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT7),
      new DeviceMapState(20, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT7),
      new DeviceMapState(21, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT6),
      new DeviceMapState(22, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT6),
      new DeviceMapState(23, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT6),
      new DeviceMapState(24, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT5),
      new DeviceMapState(25, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT5),
      new DeviceMapState(26, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT22),
      new DeviceMapState(27, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT22),
      new DeviceMapState(28, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT21),
      new DeviceMapState(29, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT21),
      new DeviceMapState(30, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT21),
      new DeviceMapState(31, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT20),
      new DeviceMapState(32, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT20),
      new DeviceMapState(33, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT20),
      new DeviceMapState(34, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT19),
      new DeviceMapState(35, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT19),
      new DeviceMapState(36, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT19),
      new DeviceMapState(37, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT18),
      new DeviceMapState(38, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT18),
      new DeviceMapState(39, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT18),
      new DeviceMapState(40, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT17),
      new DeviceMapState(41, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT17),
      new DeviceMapState(42, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT17),
      new DeviceMapState(43, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT16),
      new DeviceMapState(44, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT16),
      new DeviceMapState(45, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT16),
      new DeviceMapState(46, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT15),
      new DeviceMapState(47, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT15),
      new DeviceMapState(48, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT15),
      new DeviceMapState(49, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT14),
      new DeviceMapState(50, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT14),
      new DeviceMapState(51, Color.FromArgb(0, 0, 0), DeviceKeys.ADDITIONALLIGHT14),
    };

	bool _deviceChanged = true;
    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
    {
        try
        {
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                if (key.Key == DeviceKeys.ADDITIONALLIGHT1)
                {
					if (_deviceChanged)
						SendArgs(new byte[] { 4, 0, 0, 0, 0 });
                    SendArgs(new byte[] { 3, 0, 0, 0, 0 });
					_deviceChanged = false;
                }
                for (byte d = 0; d < deviceMap.Count; d++)
                {
                    if ((deviceMap[d].deviceKey == key.Key) && (key.Value != deviceMap[d].color))
                    {
                        SendColorToDevice(key.Value, deviceMap[d].led);
                        deviceMap[d] = new DeviceMapState(deviceMap[d].led, key.Value, deviceMap[d].deviceKey);
                        _deviceChanged = true;
                    }
                }
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    //Custom method to send the color to the device
    private void SendColorToDevice(Color color, byte led = 0)
    {
        //Check if device's current color is the same, no need to update if they are the same		
        //0 nothing
        //1 setledtrx
        //2 setledall
        //3 start trx
        //4 commit trx 
        //6 shutdown

        SendArgs(new byte[5] { 1, Convert.ToByte(color.R * color.A / 255), Convert.ToByte(color.G * color.A / 255), Convert.ToByte(color.B * color.A / 255), led });
    }
}