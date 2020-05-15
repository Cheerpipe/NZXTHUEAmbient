using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NZXTHUEAmbient
{
    public class HUE2AmbientController
    {
        private bool _transactionStarted = false;
        private Color[] _transactionColors;
        private Color _currentAllLedsColor;
        private Color[] _currentSingleLedColor;
        private int _totalLedCount;
        private IDevice _device;
        private async Task InitDevice(int totalLedCount)
        {
            WindowsHidDeviceFactory.Register(null, null);
            var deviceDefinitions = new List<FilterDeviceDefinition> { new FilterDeviceDefinition { DeviceType = DeviceType.Hid, VendorId = 0x1E71, ProductId = 0x2002, Label = "NZXT HUE 2 Ambient" } };
            var devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);
            do
            {
                Thread.Sleep(50);
            } while (devices.Count == 0);


            if (devices.Count > 0)
            {
                _device = devices.First();
            }
            else
            {
                throw new Exception("No device detected");
            }

            _totalLedCount = totalLedCount;
            Color[] _currentSingleLedColor = new Color[_totalLedCount];
    }

        public void InitDeviceSync(int totalLedCount)
        {
            InitDevice(totalLedCount).Wait(5000);
        }

        private async Task SetLeds(Color color)
        {
            if (_currentAllLedsColor == color)
                return;


            if (_device == null)
            {
                throw new Exception("No device found to send a SetLeds command");
            }

            //Create command header
            var buffer = new byte[64];
            buffer[0] = 0x28; //All leds command
            buffer[1] = 0x03; //All sub channels
            buffer[2] = 0x01; //Channel 1
            buffer[3] = 0x1c; //Unknown
            buffer[8] = 0x01; //Unknown

            //Create command payload
            buffer[10] = (byte)color.G; //G
            buffer[11] = (byte)color.R; // R
            buffer[12] = (byte)color.B; // B

            _currentAllLedsColor = color;

            await _device.InitializeAsync();

            var readBuffer = await _device.WriteAndReadAsync(buffer);

            buffer[2] = 0x02; //Channel 2
            readBuffer = await _device.WriteAndReadAsync(buffer);
        }

        public void FreeDevice()
        {
            _device.Close();
            _device.Dispose();
        }

        public bool IsInitialized()
        {
            return _device != null;
        }

        public Color[] GetCurrentColorArray()
        {
            return _currentSingleLedColor;
        }

        public void SetLedsSync(Color color)
        {
            SetLeds(color).Wait(100);
        }

        public void SetLedsSync(Color[] colors)
        {
            SetLeds(colors).Wait(100);
        }

        public void TransactionStart()
        {
            _transactionStarted = true;
            _transactionColors = (Color[])_currentSingleLedColor.Clone();
        }


        public void TransactionSetLed(int led, Color color)
        {
            if (!_transactionStarted)
                throw new Exception("Transaction not started");

            _transactionColors[led] = color;
        }

        public void TransactionCommit()
        {
            if (!_transactionStarted)
                throw new Exception("Transaction not started");
            SetLedsSync(_transactionColors);
            _transactionStarted = false;
        }

        private async Task SetLeds(Color[] colors)
        {
            if (_device == null)
            {
                throw new Exception("No device detected to send command SetLeds(Color[] newSingleLedColor)");
            }

            // Now we have to create four different commands
            //TODO: Optimize and apply commands only if at least one led changed
            // Channel 1 subchannel 1 leds 0-19
            // Channel 1 subchannel 1 leds 0-19
            var buffer = new byte[64];
            buffer[0] = 0x22; // Per led command
            buffer[1] = 0x10; // Subchannel 1
            buffer[2] = 0x01; // Channel 1
            buffer[3] = 0x00; // Unknown
            for (int i = 4; i <= 61; i = i + 3)
            {
                buffer[i] = colors[(i - 4) / 3].G;      // G
                buffer[i + 1] = colors[(i - 4) / 3].R;  // R
                buffer[i + 2] = colors[(i - 4) / 3].B;  // B
            }
            _device.InitializeAsync().Wait();
            _device.WriteAndReadAsync(buffer).Wait();

            // Channel 1 subchannel 2 leds 20-24            
            buffer = new byte[64];
            buffer[0] = 0x22; // Per led command
            buffer[1] = 0x11; // Subchannel 1
            buffer[2] = 0x01; // Channel 1
            buffer[3] = 0x00; // Unknown
            for (int i = 4; i <= 25; i = i + 3)
            {
                buffer[i] = colors[((i - 4) / 3) + 20].G; // G
                buffer[i + 1] = colors[((i - 4) / 3) + 20].R; // R
                buffer[i + 2] = colors[((i - 4) / 3) + 20].B; // B

            }
            _device.InitializeAsync().Wait();
            _device.WriteAndReadAsync(buffer).Wait();

            //Create command finalizer for channel 1
            buffer = new byte[64];
            buffer[0] = 0x22; // Per led command
            buffer[1] = 0xa0; // unknown
            buffer[2] = 0x01; // Channel 1
            buffer[3] = 0x00; // Unknown
            buffer[4] = 0x01; // Unknown
            buffer[7] = 0x1c; // Unknown
            buffer[10] = 0x80; // Unknown
            buffer[12] = 0x32; // Unknown
            buffer[15] = 0x01; // Unknown
            _device.InitializeAsync().Wait();
            _device.WriteAndReadAsync(buffer).Wait();


            // Channel 2 subchannel 1 leds 26-45
            buffer = new byte[64];
            buffer[0] = 0x22; // Per led command
            buffer[1] = 0x10; // Subchannel 1
            buffer[2] = 0x02; // Channel 2
            buffer[3] = 0x00; // Unknown
            for (int i = 4; i <= 61; i = i + 3)
            {
                /*
                buffer[i] = colors[((i - 4) / 3) + 20 + 8].R;     // R
                buffer[i + 1] = colors[((i - 4) / 3) + 20 + 8].G; // G
                buffer[i + 2] = colors[((i - 4) / 3) + 20 + 8].B; // B
                */

                //inverted
                buffer[i] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 20 + 8)].G;     // G
                buffer[i + 1] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 20 + 8)].R; // R
                buffer[i + 2] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 20 + 8)].B; // B
            }
            _device.InitializeAsync().Wait();
            _device.WriteAndReadAsync(buffer).Wait();

            // Channel 2 subchanel 2 leds 45-50 
            buffer = new byte[64];
            buffer[0] = 0x22; // Per led command
            buffer[1] = 0x11; // Subchannel 2
            buffer[2] = 0x02; // Channel 2
            buffer[3] = 0x00; // Unknown
            for (int i = 4; i <= 25; i = i + 3)
            {
                /*
               buffer[i] = colors[((i - 4) / 3) + 20 + 8 + 20].R;            // R
               buffer[i + 1] = colors[((i - 4) / 3) + 20 + 8 + 20].G;       // G
               buffer[i + 2] = colors[((i - 4) / 3) + 20 + 8 + 20].B;      // B
               */

                //inverted
                buffer[i] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 20 + 8 + 20)].G;            // G
                buffer[i + 1] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 20 + 8 + 20)].R;       // R
                buffer[i + 2] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 20 + 8 + 20)].B;      // B
            }
            _device.InitializeAsync().Wait();
            _device.WriteAndReadAsync(buffer).Wait();


            //Create command finalizer for channel 2
            buffer = new byte[64];
            buffer[0] = 0x22; // Per led command
            buffer[1] = 0xa0; // unknown
            buffer[2] = 0x02; // Channel 2
            buffer[3] = 0x00; // Unknown
            buffer[4] = 0x01; // Unknown
            buffer[7] = 0x1c; // Unknown
            buffer[10] = 0x80; // Unknown
            buffer[12] = 0x32; // Unknown
            buffer[15] = 0x01; // Unknown
            _device.InitializeAsync().Wait();
            _device.WriteAndReadAsync(buffer).Wait();

            _currentSingleLedColor = colors;
        }
    }
}
