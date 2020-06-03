using Device.Net;
using System;
using System.Collections.Generic;
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

        byte[] _commandData = new byte[LED_COMMAND_DATA_LENGHT];

        //private Color[] _newColors;

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
            _device = device;
            _device.InitializeAsync().Wait();
            DetectLedCount().Wait();
            _currentLedsColor = new Color[_totalLedCount];
            //_newColors = new Color[_totalLedCount];
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
            buffer[10] = color.G; //G
            buffer[11] = color.R; // R
            buffer[12] = color.B; // B

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
        public void SetLed(byte led, Color color)
        {
            _currentLedsColor[led] = color;
        }

        private async Task DetectLedCount()
        {
            byte[] request = new byte[64];
            request[0] = 0x20;
            request[1] = 0x03;
            request[2] = 0x00;
            byte[] response = await _device.WriteAndReadAsync(request);

            _channel1LedCount = 0;
            for (byte i = 15; i <= 20; i++) { _channel1LedCount += _stripLenghts[response[i]]; } //15 to 20 contains channel 1 device types

            _channel2LedCount = 0;
            for (byte i = 21; i <= 26; i++) { _channel2LedCount += _stripLenghts[response[i]]; } //21 to 26 contains channel 2 device types

            _totalLedCount = (byte)(_channel1LedCount + _channel2LedCount);
        }

        public void Apply()
        {
            SetLedsSync(_currentLedsColor);
        }

        //TODO: Move this to a better place
        public enum LayoutType
        {
            Circular,
            Linear,
            FromCenter
        }
        private readonly Object _SetLedsLock = new object();
        public async Task SetLeds(Color[] colors, LayoutType layoutType = LayoutType.Circular)
        {
            if (_device == null)
            {
                throw new Exception("No device detected to send command SetLeds(Color[] newSingleLedColor)");
            }

            //Fill channel 1

            lock (_SetLedsLock)
            {
                Color[] _newcolors = (Color[])colors.Clone();
                //If Linear, reverse this channel
                if (layoutType == LayoutType.Linear)
                {
                    Array.Reverse(_newcolors, 0, _channel1LedCount);
                }

                byte _commandsNeeded = (byte)Math.Ceiling(_channel1LedCount / (double)MAX_LEDS_PER_COMMAND);
                for (byte commandIndex = 0; commandIndex < 2; commandIndex++)
                {
                    byte[] _commandHeader = new byte[4]; //Header
                    _commandHeader[0] = 0x24; // Per led command
                    _commandHeader[1] = (byte)(0x04 + commandIndex); // Device 0x04 device 1 0x05 device 2 0x06 device 3...
                    _commandHeader[2] = 0x01; // Channel 0x01 channel 1 0x02 channel 2
                    _commandHeader[3] = (byte)(0x0 + commandIndex * _commandHeader[2]);

                    if (_commandsNeeded > 1)
                    {
                        int until = (((_channel1LedCount - (commandIndex * MAX_LEDS_PER_COMMAND)) / MAX_LEDS_PER_COMMAND) > 0 ? MAX_LEDS_PER_COMMAND : _channel1LedCount % MAX_LEDS_PER_COMMAND);
                        Parallel.For(0, until, dataPosition =>
                        {
                            _commandData[dataPosition * 3] = _newcolors[dataPosition + MAX_LEDS_PER_COMMAND * commandIndex].G;
                            _commandData[dataPosition * 3 + 1] = _newcolors[dataPosition + MAX_LEDS_PER_COMMAND * commandIndex].R;
                            _commandData[dataPosition * 3 + 2] = _newcolors[dataPosition + MAX_LEDS_PER_COMMAND * commandIndex].B;
                        });
                    }
                    _device.WriteAsync(_commandHeader.Concat(_commandData).ToArray()).Wait();

                }

                //Fill channel 2
                _commandsNeeded = (byte)Math.Ceiling(_channel2LedCount / (double)MAX_LEDS_PER_COMMAND);
                //If circular, reverse this channel
                if (layoutType == LayoutType.Circular)
                {
                    Array.Reverse(_newcolors, _channel1LedCount, _channel2LedCount);
                }
                for (byte commandIndex = 0; commandIndex < 2; commandIndex++)
                {
                    byte[] _commandHeader = new byte[4]; //Header
                    _commandHeader[0] = 0x24; // Per led command
                    _commandHeader[1] = (byte)(0x04 + commandIndex); // Device 0x04 device 1 0x05 device 2 0x06 device 3...
                    _commandHeader[2] = 0x02; // Channel 0x01 channel 1 0x02 channel 2
                    _commandHeader[3] = (byte)(0x0 + commandIndex * _commandHeader[2]);

                    if (_commandsNeeded > 1)
                    {
                        int until = (((_channel2LedCount - (commandIndex * MAX_LEDS_PER_COMMAND)) / MAX_LEDS_PER_COMMAND) > 0 ? MAX_LEDS_PER_COMMAND : _channel2LedCount % MAX_LEDS_PER_COMMAND);

                        Parallel.For(0, until, dataPosition =>
                        {
                            _commandData[dataPosition * 3] = _newcolors[(dataPosition + MAX_LEDS_PER_COMMAND * commandIndex) + _channel1LedCount].G;
                            _commandData[dataPosition * 3 + 1] = _newcolors[(dataPosition + MAX_LEDS_PER_COMMAND * commandIndex) + _channel1LedCount].R;
                            _commandData[dataPosition * 3 + 2] = _newcolors[(dataPosition + MAX_LEDS_PER_COMMAND * commandIndex) + _channel1LedCount].B;
                        });
                    }
                    _device.WriteAsync(_commandHeader.Concat(_commandData).ToArray()).Wait();

                }
                _currentLedsColor = colors;
                //33ms to limit fps to 30 because it is the framerate pushed by Aurora
                Thread.Sleep(33);
            }
        }
    }
}
