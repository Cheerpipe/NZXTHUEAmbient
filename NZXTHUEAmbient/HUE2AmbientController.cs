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
        private Timer _transactionMaxAliveTimer;

        private Color _currentAllLedsColor;
        private Color[] _currentLedsColor;

        private int _totalLedCount;
        private IDevice _device;

        //Todo: Allow multiple controllers and detect channel 1 and 2 array lenght
        //Todo: Functions to set easily corners (lefttop, righttop, leftbottom, rightbottom) and sides (up, down, left, right)

        public HUE2AmbientController()
        {
            _transactionMaxAliveTimer = new Timer(new TimerCallback(_transactionTimeoutTimer_Tick), null, Timeout.Infinite, Timeout.Infinite);
        }

        private void _transactionTimeoutTimer_Tick(object state)
        {
            _transactionMaxAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            if (!_transactionStarted) return;
            TransactionCancel();
        }

        public void TransactionCancel()
        {
            _transactionStarted = false;
        }

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
            _currentLedsColor = new Color[_totalLedCount];
            _device.InitializeAsync().Wait();
        }

        public void InitDeviceSync(int totalLedCount)
        {
            InitDevice(totalLedCount).Wait(5000);
        }

        public async Task SetLeds(Color color)
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
            _ = await _device.WriteAndReadAsync(buffer);

            buffer[2] = 0x02; //Channel 2
            _ = await _device.WriteAndReadAsync(buffer);
            _currentLedsColor = Enumerable.Repeat(color, _currentLedsColor.Length).ToArray(); //Refill current array with new color
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
            return _currentLedsColor;
        }

        public void SetLedsSync(Color color)
        {
            SetLeds(color).Wait(100);
        }

        public void SetLedsSync(Color[] colors)
        {
            SetLeds(colors).Wait(100);
        }

        public void TransactionStart(int transactionTimeout = Timeout.Infinite)
        {
            _transactionStarted = true;
            _transactionColors = (Color[])_currentLedsColor.Clone();
            if (transactionTimeout > 0)
            {
                _transactionMaxAliveTimer.Change(transactionTimeout, transactionTimeout);
            }
        }

        public void TransactionSetLed(int led, Color color)
        {
            _transactionColors[led] = color;
        }

        public void TransactionCommit()
        {
            if (!_transactionStarted)
                return;
            SetLedsSync(_transactionColors);
            _transactionStarted = false;
        }

        //TODO: Allow non symetrical arrays. For now, it will only work with symetrical channels
        public async Task SetLeds(Color[] colors)
        {
            if (_device == null)
            {
                throw new Exception("No device detected to send command SetLeds(Color[] newSingleLedColor)");
            }

            // Now we have to create four different commands
            //TODO: Optimize and apply commands only if at least one led changed
            // Channel 1 subchannel 1 leds 0-19
            // Channel 1 subchannel 1 leds 0-19
            var bufferC1S1 = new byte[64]; //Always 20 leds per command
            bufferC1S1[0] = 0x22; // Per led command
            bufferC1S1[1] = 0x10; // Subchannel 1
            bufferC1S1[2] = 0x01; // Channel 1
            bufferC1S1[3] = 0x00; // Unknown

            for (int i = 4; i <= 61; i = i + 3) //Fill entire array
            {
                bufferC1S1[i] = colors[(i - 4) / 3].G;      // G
                bufferC1S1[i + 1] = colors[(i - 4) / 3].R;  // R
                bufferC1S1[i + 2] = colors[(i - 4) / 3].B;  // B
            }

            // Channel 1 subchannel 2 leds 20-24   
            var bufferC1S2 = new byte[64];
            if (_totalLedCount > 40) // 20 is the max capacity per command. If we have more leds, we need second part command
            {
                bufferC1S2[0] = 0x22; // Per led command
                bufferC1S2[1] = 0x11; // Subchannel 1
                bufferC1S2[2] = 0x01; // Channel 1
                bufferC1S2[3] = 0x00; // Unknown
                for (int i = 4; i <= (_totalLedCount / 2 - 20) * 3 + 1; i = i + 3) //Fill the remaining array
                {
                    //((i - 4) / 3) + 20 => (i + 56) / 3
                    bufferC1S2[i] = colors[(i + 56) / 3].G; // G
                    bufferC1S2[i + 1] = colors[(i + 56) / 3].R; // R
                    bufferC1S2[i + 2] = colors[(i + 56) / 3].B; // B
                }
            }
            //Create command finalizer for channel 1
            var bufferC1F = new byte[64];
            bufferC1F = new byte[64];
            bufferC1F[0] = 0x22; // Per led command
            bufferC1F[1] = 0xa0; // unknown
            bufferC1F[2] = 0x01; // Channel 1
            bufferC1F[3] = 0x00; // Unknown
            bufferC1F[4] = 0x01; // Unknown
            bufferC1F[7] = 0x1c; // Unknown
            bufferC1F[10] = 0x80; // Unknown
            bufferC1F[12] = 0x32; // Unknown
            bufferC1F[15] = 0x01; // Unknown

            _ = await _device.WriteAndReadAsync(bufferC1S1);
            _ = await _device.WriteAndReadAsync(bufferC1S2);
            _ = await _device.WriteAndReadAsync(bufferC1F);

            // Channel 2 subchannel 1 leds 26-45
            var bufferC2S1 = new byte[64];
            bufferC2S1[0] = 0x22; // Per led command
            bufferC2S1[1] = 0x10; // Subchannel 1
            bufferC2S1[2] = 0x02; // Channel 2
            bufferC2S1[3] = 0x00; // Unknown
            for (int i = 4; i <= 61; i = i + 3)
            { //(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 20 + 8) = (colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 28)
                bufferC2S1[i] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 20 + 8)].G;     // G
                bufferC2S1[i + 1] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 28)].R; // R
                bufferC2S1[i + 2] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 28)].B; // B
            }

            // Channel 2 subchanel 2 leds 45-50 
            var bufferC2S2 = new byte[64];
            if (_totalLedCount > 40) // 20 is the max capacity per command. If we have more than 40 leds we have mor than 20 leds per chanel so we need second part command
            {
                bufferC2S2[0] = 0x22; // Per led command
                bufferC2S2[1] = 0x11; // Subchannel 2
                bufferC2S2[2] = 0x02; // Channel 2
                bufferC2S2[3] = 0x00; // Unknown
                for (int i = 4; i <= (_totalLedCount / 2 - 20) * 3 + 1; i = i + 3)
                { //(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 20 + 8 + 20)
                    bufferC2S2[i] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 48)].G;            // G
                    bufferC2S2[i + 1] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 48)].R;       // R
                    bufferC2S2[i + 2] = colors[(colors.Length + (colors.Length / 2) - 1) - (((i - 4) / 3) + 48)].B;      // B
                }
            }

            //Create command finalizer for channel 2
            var bufferC2F = new byte[64];
            bufferC2F[0] = 0x22; // Per led command
            bufferC2F[1] = 0xa0; // unknown
            bufferC2F[2] = 0x02; // Channel 2
            bufferC2F[3] = 0x00; // Unknown
            bufferC2F[4] = 0x01; // Unknown
            bufferC2F[7] = 0x1c; // Unknown
            bufferC2F[10] = 0x80; // Unknown
            bufferC2F[12] = 0x32; // Unknown
            bufferC2F[15] = 0x01; // Unknown

            _ = await _device.WriteAndReadAsync(bufferC2S1);
            _ = await _device.WriteAndReadAsync(bufferC2S2);
            _ = await _device.WriteAndReadAsync(bufferC2F);
            _currentLedsColor = colors;
        }
    }
}
