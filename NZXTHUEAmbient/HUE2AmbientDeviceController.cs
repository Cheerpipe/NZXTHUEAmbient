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

        private byte _channel1LedCount;
        private byte _channel2LedCount;

        public byte Channel1LedCount { get => _channel1LedCount; }
        public byte Channel2LedCount { get => _channel2LedCount; }
        public byte TotalLedCount { get => _totalLedCount; }
        
        public HUE2AmbientDeviceController(IDevice device)
        {
            _transactionMaxAliveTimer = new Timer(new TimerCallback(_transactionTimeoutTimer_Tick), null, Timeout.Infinite, Timeout.Infinite);
            _device = device;
            _device.InitializeAsync().Wait();
            DetectLedCount().Wait();
            _currentLedsColor = new Color[_totalLedCount];
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

        private async Task DetectLedCount()
        {
            // 0x04 = 10 led strip
            // 0x05 = 8 led strip
            //TODO Move this to a better place
            Dictionary<byte, byte> _stripLenghts = new System.Collections.Generic.Dictionary<byte, byte>();
            _stripLenghts.Add(0x00, 0);
            _stripLenghts.Add(0x04, 10);
            _stripLenghts.Add(0x05, 8);

            byte[] request = new byte[64];
            request[0] = 0x20;
            request[1] = 0x03;
            request[2] = 0x00;
            byte[] response = await _device.WriteAndReadAsync(request);

            _channel1LedCount = (byte)(
               _stripLenghts[response[15]] +
               _stripLenghts[response[16]] +
               _stripLenghts[response[17]] +
               _stripLenghts[response[18]] +
               _stripLenghts[response[19]] +
               _stripLenghts[response[20]]);

            _channel2LedCount = (byte)(
               _stripLenghts[response[21]] +
               _stripLenghts[response[22]] +
               _stripLenghts[response[23]] +
               _stripLenghts[response[24]] +
               _stripLenghts[response[25]] +
               _stripLenghts[response[26]]);
            _totalLedCount = (byte)(_channel1LedCount + _channel2LedCount);
        }

        public void TransactionCommit()
        {
            if (!_transactionStarted)
                return;
            SetLedsSync(_transactionColors);
            _transactionStarted = false;
        }

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
            int c1s1MaxLenght = Channel1LedCount > 20 ? 21 : Channel1LedCount + 1;
            Parallel.For(1, c1s1MaxLenght, i =>
                   {
                       bufferC1S1[(i * 3 + 1)] = colors[((i * 3 + 1) - 4) / 3].G;      // G
                       bufferC1S1[(i * 3 + 1) + 1] = colors[((i * 3 + 1) - 4) / 3].R;  // R
                       bufferC1S1[(i * 3 + 1) + 2] = colors[((i * 3 + 1) - 4) / 3].B;  // B
                   });

            // Channel 1 subchannel 2 leds 20-24   
            var bufferC1S2 = new byte[64];
            if (TotalLedCount > 40) // 20 is the max capacity per command. If we have more leds, we need second part command
            {
                bufferC1S2[0] = 0x24; // Per led command
                bufferC1S2[1] = 0x05; // Subchannel 1
                bufferC1S2[2] = 0x01; // Channel 1
                bufferC1S2[3] = 0x01; // Unknown
                int c1s2MaxLenght = _channel1LedCount - 20 + 1;
                Parallel.For(1, c1s2MaxLenght, i =>
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
            int c2s1MaxLenght = Channel2LedCount > 20 ? 21 : Channel2LedCount + 1;
            Parallel.For(1, c2s1MaxLenght, i =>
             {
                 bufferC2S1[(i * 3 + 1)] = colors[(colors.Length + (colors.Length / 2) - 1) - ((((i * 3 + 1) - 4) / 3) + 20 + 8)].G;     // G
                 bufferC2S1[(i * 3 + 1) + 1] = colors[(colors.Length + (colors.Length / 2) - 1) - ((((i * 3 + 1) - 4) / 3) + 28)].R; // R
                 bufferC2S1[(i * 3 + 1) + 2] = colors[(colors.Length + (colors.Length / 2) - 1) - ((((i * 3 + 1) - 4) / 3) + 28)].B; // B
             });

            // Channel 2 subchanel 2 leds 45-50 
            var bufferC2S2 = new byte[64];
            if (TotalLedCount > 40) // 20 is the max capacity per command. If we have more than 40 leds we have mor than 20 leds per chanel so we need second part command
            {
                bufferC2S2[0] = 0x24; // Per led command
                bufferC2S2[1] = 0x05; // Subchannel 2
                bufferC2S2[2] = 0x02; // Channel 2
                bufferC2S2[3] = 0x02; // Unknown
                int c2s2MaxLenght = _channel2LedCount - 20 + 1;
                Parallel.For(1, c2s2MaxLenght, i =>
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
