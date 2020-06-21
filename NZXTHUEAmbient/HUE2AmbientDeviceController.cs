using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NZXTHUEAmbient
{
    public class HUE2AmbientDeviceController : IDisposable
    {
        //Move this to a better place
        private static Dictionary<byte, byte> _stripLenghts = new System.Collections.Generic.Dictionary<byte, byte>() {
            {0x00, 0},
            {0x04, 10}, // 4 = 10 led strip
            {0x05, 8}}; // 5 = 10 led strip

        private const int MAX_LEDS_PER_COMMAND = 20;
        private const int LED_COMMAND_DATA_LENGHT = MAX_LEDS_PER_COMMAND * 3;

        private string _hidDeviceId = string.Empty;

        byte[] _commandData = new byte[LED_COMMAND_DATA_LENGHT];

        byte[] _commandHeader = new byte[4]; //Header

        //private Color[] _newColors;

        private Color _currentAllLedsColor;
        private Color[] _currentLedsColor;

        private int _totalLedCount;
        private WindowsHidDevice _device;

        private int _channel1LedCount;
        private int _channel2LedCount;
        public int Channel1LedCount { get => _channel1LedCount; }
        public int Channel2LedCount { get => _channel2LedCount; }
        public int TotalLedCount { get => _totalLedCount; }

        public HUE2AmbientDeviceController(IDevice device, bool useLastSetting = false)
        {
            Initialize(device, useLastSetting);
        }
        public void Initialize(IDevice device, bool useLastSetting = false)
        {
            _device = (WindowsHidDevice)device;
            _device.InitializeAsync().Wait();
            _hidDeviceId = Regex.Match(_device.DeviceId, @"&pid_2002#7&(.+?)&").Groups[1].Value;
            if (_totalLedCount == 0)
                DetectLedCount(useLastSetting).Wait();
            _currentLedsColor = new Color[_totalLedCount];
            _commandHeader[0] = 0x24; // Direct  led command
        }

        public void ReInitialize()
        {
            Initialize(_device);
        }

        public void SetLeds(Color color)
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
            _device.WriteAsync(buffer).Wait(100);

            buffer[2] = 0x02; //Channel 2
            _device.WriteAsync(buffer).Wait(100);
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

        public void SetLed(byte led, Color color)
        {
            _currentLedsColor[led] = color;
        }

        private void SaveLedCountToRegistry(int channel, int ledCount)
        {
            var reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\NZXT HUE Controller\" + _hidDeviceId, true);
            if (reg == null)
                reg = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\NZXT HUE Controller\" + _hidDeviceId);
            reg.SetValue("Channel_" + channel, ledCount);
        }

        private int GetLedCountFromRegistry(int channel)
        {
            var reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\NZXT HUE Controller\" + _hidDeviceId, true);
            if (reg == null)
                return -1;
            return (int)reg.GetValue("Channel_" + channel, -1);
        }

        //TODO cache led count to start up device init and avoid black screen
        private async Task DetectLedCount(bool useLastSetting = false)
        {

            if (useLastSetting)
            {
                _channel1LedCount = GetLedCountFromRegistry(1);
                _channel2LedCount = GetLedCountFromRegistry(2);
            }

            if (_channel1LedCount == -1 || _channel2LedCount == -1)
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

                SaveLedCountToRegistry(1, _channel1LedCount);
                SaveLedCountToRegistry(2, _channel1LedCount);

            }
            _totalLedCount = (byte)(_channel1LedCount + _channel2LedCount);
        }

        public void Apply()
        {
            SetLeds(_currentLedsColor);
        }

        //TODO: Move this to a better place
        public enum LayoutType
        {
            Circular,
            Linear,
            FromCenter
        }

        private readonly Object _SetLedsLock = new object();
        public void SetLeds(Color[] colors, LayoutType layoutType = LayoutType.Circular)
        {
            if (_device == null)
            {
                throw new Exception("No device detected to send command SetLeds(Color[] newSingleLedColor)");
            }
            //Fill channel 1
            int maxLedIndexForCommand;
            Thread.Sleep(16);
            Color[] _newcolors = (Color[])colors.Clone();
            //If Linear, reverse this channel
            if (layoutType == LayoutType.Linear)
            {
                Array.Reverse(_newcolors, 0, _channel1LedCount);
            }

            int _commandsNeeded = (int)(_channel1LedCount / MAX_LEDS_PER_COMMAND);
            _commandHeader[2] = 0x01; // Channel 0x01 channel 1 0x02 channel 2
            for (int commandIndex = 0; commandIndex < 2; commandIndex++)
            {
                _commandHeader[1] = (byte)(0x04 + commandIndex); // Device 0x04 device 1 0x05 device 2 0x06 device 3...
                _commandHeader[3] = (byte)(commandIndex * _commandHeader[2]);


                maxLedIndexForCommand = (((_channel1LedCount - (commandIndex * MAX_LEDS_PER_COMMAND)) / MAX_LEDS_PER_COMMAND) > 0 ? MAX_LEDS_PER_COMMAND : _channel1LedCount % MAX_LEDS_PER_COMMAND);
                //Parallel.For(0, until, dataPosition =>
                for (int dataPosition = 0; dataPosition < maxLedIndexForCommand; dataPosition++)
                {
                    _commandData[dataPosition * 3] = _newcolors[dataPosition + MAX_LEDS_PER_COMMAND * commandIndex].G;
                    _commandData[dataPosition * 3 + 1] = _newcolors[dataPosition + MAX_LEDS_PER_COMMAND * commandIndex].R;
                    _commandData[dataPosition * 3 + 2] = _newcolors[dataPosition + MAX_LEDS_PER_COMMAND * commandIndex].B;
                }//);
                _ = _device.WriteAsync(_commandHeader.Concat(_commandData).ToArray()).Wait(100);
            }

            //Fill channel 2
            _commandsNeeded = (int)(_channel2LedCount / MAX_LEDS_PER_COMMAND);
            //If circular, reverse this channel
            if (layoutType == LayoutType.Circular)
            {
                Array.Reverse(_newcolors, _channel1LedCount, _channel2LedCount);
            }

            _commandHeader[2] = 0x02; // Channel 0x01 channel 1 0x02 channel 2
            for (int commandIndex = 0; commandIndex < 2; commandIndex++)
            {
                _commandHeader[1] = (byte)(0x04 + commandIndex); // Device 0x04 device 1 0x05 device 2 0x06 device 3...
                _commandHeader[3] = (byte)(commandIndex * _commandHeader[2]);


                maxLedIndexForCommand = (((_channel2LedCount - (commandIndex * MAX_LEDS_PER_COMMAND)) / MAX_LEDS_PER_COMMAND) > 0 ? MAX_LEDS_PER_COMMAND : _channel2LedCount % MAX_LEDS_PER_COMMAND);

                //Parallel.For(0, until, dataPosition =>
                for (int dataPosition = 0; dataPosition < maxLedIndexForCommand; dataPosition++)
                {
                    _commandData[dataPosition * 3] = _newcolors[(dataPosition + MAX_LEDS_PER_COMMAND * commandIndex) + _channel1LedCount].G;
                    _commandData[dataPosition * 3 + 1] = _newcolors[(dataPosition + MAX_LEDS_PER_COMMAND * commandIndex) + _channel1LedCount].R;
                    _commandData[dataPosition * 3 + 2] = _newcolors[(dataPosition + MAX_LEDS_PER_COMMAND * commandIndex) + _channel1LedCount].B;
                }//);

                _ = _device.WriteAsync(_commandHeader.Concat(_commandData).ToArray()).Wait(100);
            }
            _currentLedsColor = colors;
            if (_device.IsInitialized == false)
            {
                throw new Exception("Unknown error 001");
            }
        }

        public void Dispose()
        {
            _device.Dispose();
        }
    }
}
