using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NZXTHUEAmbient
{
    public class HUE2AmbientDeviceController
    {
        private bool _transactionStarted = false;
        private Color[] _transactionColors;
        private Timer _transactionMaxAliveTimer;

        private Color _currentAllLedsColor;
        private Color[] _currentLedsColor;

        private byte _totalLedCount;
        private IDevice _device;

        private int _channel1LedCount;
        private int _channel2LedCount;

        //TODO: Autodetected and readonly.Seteable only for now
        public int Channel1LedCount { get => _channel1LedCount; set => _channel1LedCount = value; }
        public int Channel2LedCount { get => _channel2LedCount; set => _channel2LedCount = value; }

        //Todo: Allow multiple controllers and detect channel 1 and 2 array lenght

        public HUE2AmbientDeviceController(IDevice device)
        {
            //TODO: This number must be detected
            _totalLedCount = 56;
            _transactionMaxAliveTimer = new Timer(new TimerCallback(_transactionTimeoutTimer_Tick), null, Timeout.Infinite, Timeout.Infinite);
            _currentLedsColor = new Color[_totalLedCount];
            _device = device;
            _device.InitializeAsync().Wait();
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

        /*
        private async Task InitDevices(byte totalLedCount, int deviceIndex)
        {
            WindowsHidDeviceFactory.Register(null, null);
            var deviceDefinitions = new List<FilterDeviceDefinition> { new FilterDeviceDefinition { DeviceType = DeviceType.Hid, VendorId = 0x1E71, ProductId = 0x2002, Label = "NZXT HUE 2 Ambient" } };
            List<IDevice> devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);
            _devices = devices.ToArray();

            if (_devices.Length < 0)
            {
                throw new Exception("No device detected");
            }

            _totalLedCount = totalLedCount;
            _currentLedsColor = new Color[_totalLedCount];
            await _device.InitializeAsync();
        }
        */
        /*
        public void InitDeviceSync(byte totalLedCount)
        {
            InitDevices(totalLedCount, 0).Wait(5000);
        }
        */
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
            await _device.WriteAsync(buffer);

            buffer[2] = 0x02; //Channel 2
            await _device.WriteAsync(buffer);
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

        public void TransactionSetLed(byte led, Color color)
        {
            if (_transactionColors == null)
                TransactionStart();
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
            // Channel 1 subchannel 1 leds 0-19
            byte[] bufferC1S1 = new byte[64]; //Always 20 leds per command
            bufferC1S1[0] = 0x24; // Per led command
            bufferC1S1[1] = 0x04; // Subchannel 1
            bufferC1S1[2] = 0x01; // Channel 1
            bufferC1S1[3] = 0x00; // Unknown
            Parallel.For(1, 21, i =>
                   {
                       bufferC1S1[(i * 3 + 1)] = colors[((i * 3 + 1) - 4) / 3].G;      // G
                       bufferC1S1[(i * 3 + 1) + 1] = colors[((i * 3 + 1) - 4) / 3].R;  // R
                       bufferC1S1[(i * 3 + 1) + 2] = colors[((i * 3 + 1) - 4) / 3].B;  // B
                   });

            // Channel 1 subchannel 2 leds 20-24   
            var bufferC1S2 = new byte[64];
            if (_totalLedCount > 40) // 20 is the max capacity per command. If we have more leds, we need second part command
            {
                bufferC1S2[0] = 0x24; // Per led command
                bufferC1S2[1] = 0x05; // Subchannel 1
                bufferC1S2[2] = 0x01; // Channel 1
                bufferC1S2[3] = 0x01; // Unknown
                Parallel.For(1, 11, i =>
                 {
                     bufferC1S2[(i * 3 + 1)] = colors[((i * 3 + 1) + 56) / 3].G; // G
                     bufferC1S2[(i * 3 + 1) + 1] = colors[((i * 3 + 1) + 56) / 3].R; // R
                     bufferC1S2[(i * 3 + 1) + 2] = colors[((i * 3 + 1) + 56) / 3].B; // B
                 });
            }

            // Channel 2 subchannel 1 leds 26-45
            var bufferC2S1 = new byte[64];
            bufferC2S1[0] = 0x24; // Per led command
            bufferC2S1[1] = 0x04; // Subchannel 1
            bufferC2S1[2] = 0x02; // Channel 2
            bufferC2S1[3] = 0x00; // Unknown
            Parallel.For(1, 21, i =>
             {
                 bufferC2S1[(i * 3 + 1)] = colors[(colors.Length + (colors.Length / 2) - 1) - ((((i * 3 + 1) - 4) / 3) + 20 + 8)].G;     // G
                 bufferC2S1[(i * 3 + 1) + 1] = colors[(colors.Length + (colors.Length / 2) - 1) - ((((i * 3 + 1) - 4) / 3) + 28)].R; // R
                 bufferC2S1[(i * 3 + 1) + 2] = colors[(colors.Length + (colors.Length / 2) - 1) - ((((i * 3 + 1) - 4) / 3) + 28)].B; // B
             });

            // Channel 2 subchanel 2 leds 45-50 
            var bufferC2S2 = new byte[64];
            if (_totalLedCount > 40) // 20 is the max capacity per command. If we have more than 40 leds we have mor than 20 leds per chanel so we need second part command
            {
                bufferC2S2[0] = 0x24; // Per led command
                bufferC2S2[1] = 0x05; // Subchannel 2
                bufferC2S2[2] = 0x02; // Channel 2
                bufferC2S2[3] = 0x02; // Unknown
                Parallel.For(1, 11, i =>
                 {
                     bufferC2S2[(i * 3 + 1)] = colors[(colors.Length + (colors.Length / 2) - 1) - ((((i * 3 + 1) - 4) / 3) + 48)].G;            // G
                     bufferC2S2[(i * 3 + 1) + 1] = colors[(colors.Length + (colors.Length / 2) - 1) - ((((i * 3 + 1) - 4) / 3) + 48)].R;       // R
                     bufferC2S2[(i * 3 + 1) + 2] = colors[(colors.Length + (colors.Length / 2) - 1) - ((((i * 3 + 1) - 4) / 3) + 48)].B;      // B
                 });
            }

            //Set channel 1 subchannel 1 and 2
            await _device.WriteAsync(bufferC1S1);
            await _device.WriteAsync(bufferC1S2);

            //Set channel 2 subchannel 1 and 2
            await _device.WriteAsync(bufferC2S1);
            await _device.WriteAsync(bufferC2S2);

            _currentLedsColor = colors;
        }
    }
}
