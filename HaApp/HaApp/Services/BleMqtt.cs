using Plugin.BLE;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HaApp.Services
{
    public class BleMqtt
    {
        public async void Start()
        {
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            ble.StateChanged += (s, e) =>
            {
                Debug.WriteLine($"The bluetooth state changed to {e.NewState}");
            };
            adapter.DeviceDiscovered += async (s, a) => {
                string mac = "00000000-0000-0000-0000-683e34cce53f";
                if (a.Device.Id.ToString() == mac)
                {
                    try
                    {
                        var connectedDevice = await adapter.ConnectToKnownDeviceAsync(Guid.Parse("00000000-0000-0000-0000-683e34cce53f"));
                        var services = await connectedDevice.GetServicesAsync();
                        foreach (var sv in services)
                        {
                            Debug.WriteLine($"服务 {sv.Name} {sv.Id}");
                        }
                        /*
                        [0:] 服务 Generic Access 00001800-0000-1000-8000-00805f9b34fb
                        [0:] 服务 Generic Attribute 00001801-0000-1000-8000-00805f9b34fb
                        [0:] 服务 Device Information 0000180a-0000-1000-8000-00805f9b34fb
                        [0:] 服务 Unknown Service 0000fef5-0000-1000-8000-00805f9b34fb
                        [0:] 服务 Unknown Service 000016f0-0000-1000-8000-00805f9b34fb
                        [0:] 蓝牙设备 测试 00000000-0000-0000-0000-683e34cce53f
                         */
                        // 获得特定服务
                        var service = await connectedDevice.GetServiceAsync(Guid.Parse("000016f0-0000-1000-8000-00805f9b34fb"));
                        var chs = await service.GetCharacteristicsAsync();
                        foreach (var ch in chs)
                        {
                            Debug.WriteLine($"特征 {ch.Name} {ch.Id}");
                        }
                        // 获得特定特征
                        var characteristic = await service.GetCharacteristicAsync(Guid.Parse("000016f2-0000-1000-8000-00805f9b34fb"));
                        // 读取温湿度
                        await characteristic.WriteAsync(new byte[] { 85, 3, 8, 17 });
                        var buffer = await characteristic.ReadAsync();
                        if (buffer != null && buffer.Length == 8)
                        {
                            byte[] temphex = new byte[] { buffer[4], buffer[5] };
                            byte[] humihex = new byte[] { buffer[6], buffer[7] };
                            Debug.WriteLine("温度：{0}", BitConverter.ToInt16(temphex, 0) / 100.0f);
                            Debug.WriteLine("湿度：{0}", BitConverter.ToInt16(humihex, 0) / 100.0f);

                        }
                        // 读取电量
                        await characteristic.WriteAsync(new byte[] { 85, 3, 8, 16 });
                        buffer = await characteristic.ReadAsync();
                        if (buffer != null && buffer.Length == 5)
                        {
                            Debug.WriteLine("电压：{0}", buffer[4] / 10.0f);
                        }
                    }
                    catch (DeviceConnectionException e)
                    {
                        Debug.WriteLine(e);
                        // ... could not connect to device
                    }
                    Debug.WriteLine($"蓝牙设备 {a.Device.Name} {a.Device.Id}");

                }
            };
            await adapter.StartScanningForDevicesAsync();
        }
    }
}
