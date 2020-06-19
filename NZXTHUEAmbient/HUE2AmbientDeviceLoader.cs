using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace NZXTHUEAmbient
{
    public static class HUE2AmbientDeviceLoader
    {
        private static IDevice[] _devices;

        private static HUE2AmbientDeviceController[] _HUE2AmbientDeviceController;
        public static HUE2AmbientDeviceController[] Devices { get => _HUE2AmbientDeviceController; }

        public static async Task InitDevice(int deviceIndex)
        {
            WindowsHidDeviceFactory.Register(null, null);
            List<FilterDeviceDefinition> deviceDefinitions = new List<FilterDeviceDefinition>();
            FilterDeviceDefinition d = new FilterDeviceDefinition { DeviceType = DeviceType.Hid, VendorId = 0x1E71, ProductId = 0x2002, Label = "NZXT HUE 2 Ambient " };
            deviceDefinitions.Add(d);
            List<IDevice> devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);
            _devices = devices.ToArray();
            if (deviceIndex > _devices.Length)
                throw new Exception("Device index is grater than device count");
            _HUE2AmbientDeviceController = new HUE2AmbientDeviceController[_devices.Length];

            // for (byte i = 0; i < _devices.Length; i++)
            // {
            _HUE2AmbientDeviceController[deviceIndex] = new HUE2AmbientDeviceController(_devices[deviceIndex]);
            // }
        }
    }
}
