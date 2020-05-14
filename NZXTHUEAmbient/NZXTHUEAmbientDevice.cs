using Device.Net;
using Hid.Net.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace NZXTHUEAmbient
{
    public class NZXTHUEAmbientDevice
    {
        private IDevice _device;

        public void RegisterDeviceFactory()
        {
            WindowsHidDeviceFactory.Register(null, null);

        }

        public async Task InitDevice()
        {

            var deviceDefinitions = new List<FilterDeviceDefinition> { new FilterDeviceDefinition { DeviceType = DeviceType.Hid, VendorId = 0x1E71, ProductId = 0x2002, Label = "NZXT HUE 2 Ambient" } };
            var devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);
            do
            {
                Thread.Sleep(100);
            } while (devices.Count == 0);


            if (devices.Count > 0)
                _device = devices.First();
            else
                _device = null;
        }

        public void InitDeviceSync()
        {
            InitDevice().Wait(5000);
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

        public void SetAllLedsSync(byte R, byte G, byte B)
        {
            SetAllLeds(R, G, B).Wait(100);
        }


        public async Task SetAllLeds(byte R, byte G, byte B)
        {
            if (_device == null)
            {
                return;
            }

            var buffer = new byte[64];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0x00;
            }
            //Just for my setup, still didn't stud
            buffer[0] = 0x28;
            buffer[1] = 0x03;
            buffer[2] = 0x01; //Channel 1
            buffer[3] = 0x1c;
            buffer[8] = 0x01;
            buffer[10] = G; //G
            buffer[11] = R; // R
            buffer[12] = B; // B
            await _device.InitializeAsync();
            var readBuffer = await _device.WriteAndReadAsync(buffer);
            buffer[2] = 0x02; //Channel 2
            readBuffer = await _device.WriteAndReadAsync(buffer);
        }
    }
}
