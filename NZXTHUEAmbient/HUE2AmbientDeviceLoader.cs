using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace NZXTHUEAmbient
{
    public static class HUE2AmbientDeviceLoader
    {
        private static IDevice[] _devices;

        private static HUE2AmbientDeviceController[] _HUE2AmbientDeviceController;
        public static HUE2AmbientDeviceController[] Devices { get => _HUE2AmbientDeviceController; }

        public static async Task InitDevices()
        {
            //WindowsUsbDeviceFactory.Register(null, null);
            WindowsHidDeviceFactory.Register(null, null);
            List<FilterDeviceDefinition> deviceDefinitions = new List<FilterDeviceDefinition>();
            FilterDeviceDefinition d = new FilterDeviceDefinition { DeviceType = DeviceType.Hid, VendorId = 0x1E71, ProductId = 0x2002, Label = "NZXT HUE 2 Ambient " };
            deviceDefinitions.Add(d);
            List<IDevice> devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);
            _devices = devices.ToArray();
            _HUE2AmbientDeviceController = new HUE2AmbientDeviceController[_devices.Length];

            for (byte i = 0; i < _devices.Length; i++)
            {
                _HUE2AmbientDeviceController[i] = new HUE2AmbientDeviceController(_devices[i]);
            }
        }
    }
}
