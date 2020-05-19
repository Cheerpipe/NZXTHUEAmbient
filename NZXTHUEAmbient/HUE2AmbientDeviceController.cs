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
        //Move this to a better place
        private static Dictionary<byte, byte> _stripLenghts = new System.Collections.Generic.Dictionary<byte, byte>() {
            {0x00, 0},
            {0x04, 10}, // 4 = 10 led strip
            {0x05, 8}}; // 5 = 10 led strip

        private const int MAX_LEDS_PER_COMMAND = 20;
        private const int LED_COMMAND_DATA_LENGHT = MAX_LEDS_PER_COMMAND * 3;

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

        //TODO: Move this to a better place
        public enum LayoutType
        {
            Circular,
            Linear,
            FromCenter
        }

        public async Task SetLeds(Color[] colors, LayoutType layoutType = LayoutType.Circular)
        {
            if (_device == null)
            {
                throw new Exception("No device detected to send command SetLeds(Color[] newSingleLedColor)");
            }

            //Fill channel 1

            Color[] _newcolors = (Color[])colors.Clone();
            //If Linear, reverse this channel
            if (layoutType == LayoutType.Linear)
            {
                Array.Reverse(_newcolors, 0, _channel1LedCount);
            }

            byte _commandsNeeded = (byte)Math.Ceiling(_channel1LedCount / (double)MAX_LEDS_PER_COMMAND);
            for (byte commandIndex = 0; commandIndex < _commandsNeeded; commandIndex++)
            {
                byte[] _commandHeader = new byte[4]; //Header
                _commandHeader[0] = 0x24; // Per led command
                _commandHeader[1] = (byte)(0x04 + commandIndex); // Device 0x04 device 1 0x05 device 2 0x06 device 3...
                _commandHeader[2] = 0x01; // Channel 0x01 channel 1 0x02 channel 2
                _commandHeader[3] = (byte)(0x0 + commandIndex * _commandHeader[2]);
                byte[] _commandData = new byte[LED_COMMAND_DATA_LENGHT]; //Always 20 leds per command
                Parallel.For(0, (((_channel1LedCount - (commandIndex * MAX_LEDS_PER_COMMAND)) / MAX_LEDS_PER_COMMAND) > 0 ? MAX_LEDS_PER_COMMAND : _channel1LedCount % MAX_LEDS_PER_COMMAND), dataPosition =>
                {
                    _commandData[dataPosition * 3] = _newcolors[dataPosition + MAX_LEDS_PER_COMMAND * commandIndex].G;
                    _commandData[dataPosition * 3 + 1] = _newcolors[dataPosition + MAX_LEDS_PER_COMMAND * commandIndex].R;
                    _commandData[dataPosition * 3 + 2] = _newcolors[dataPosition + MAX_LEDS_PER_COMMAND * commandIndex].B;
                });
                _device.WriteAsync(_commandHeader.Concat(_commandData).ToArray()).Wait();
            }

            //Fill channel 2
            _commandsNeeded = (byte)Math.Ceiling(_channel2LedCount / (double)MAX_LEDS_PER_COMMAND);
            //If circular, reverse this channel
            if (layoutType == LayoutType.Circular)
            {
                Array.Reverse(_newcolors, _channel1LedCount, _channel2LedCount);
            }
            for (byte commandIndex = 0; commandIndex < _commandsNeeded; commandIndex++)
            {
                byte[] _commandHeader = new byte[4]; //Header
                _commandHeader[0] = 0x24; // Per led command
                _commandHeader[1] = (byte)(0x04 + commandIndex); // Device 0x04 device 1 0x05 device 2 0x06 device 3...
                _commandHeader[2] = 0x02; // Channel 0x01 channel 1 0x02 channel 2
                _commandHeader[3] = (byte)(0x0 + commandIndex * _commandHeader[2]);
                byte[] _commandData = new byte[LED_COMMAND_DATA_LENGHT]; //Always 20 leds per command
                Parallel.For(0, (((_channel2LedCount - (commandIndex * MAX_LEDS_PER_COMMAND)) / MAX_LEDS_PER_COMMAND) > 0 ? MAX_LEDS_PER_COMMAND : _channel2LedCount % MAX_LEDS_PER_COMMAND), dataPosition =>
                {
                    _commandData[dataPosition * 3] = _newcolors[(dataPosition + MAX_LEDS_PER_COMMAND * commandIndex) + _channel1LedCount].G;
                    _commandData[dataPosition * 3 + 1] = _newcolors[(dataPosition + MAX_LEDS_PER_COMMAND * commandIndex) + _channel1LedCount].R;
                    _commandData[dataPosition * 3 + 2] = _newcolors[(dataPosition + MAX_LEDS_PER_COMMAND * commandIndex) + _channel1LedCount].B;
                });
                _device.WriteAsync(_commandHeader.Concat(_commandData).ToArray()).Wait();
            }
            _currentLedsColor = colors;
        }
    }
}
