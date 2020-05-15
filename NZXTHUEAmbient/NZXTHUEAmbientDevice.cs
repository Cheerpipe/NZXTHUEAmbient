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
            {
                _device = devices.First();
               // await _device.InitializeAsync();
            }
            else
                _device = null;

        }

    }
}
