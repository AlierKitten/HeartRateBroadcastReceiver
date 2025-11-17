using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace HeartRateBroadcast
{
    public class HeartRateMonitor
    {
        // 设备名称
        private const string DeviceName = "Xiaomi";
        
        // 心率值
        private static int heartRateValue = 0;
        
        // 最大连接超时时间（秒）
        private const int Timeout = 10;
        
        // 断开连接事件
        private static TaskCompletionSource<bool> disconnectedEvent;

        // 通知处理器函数，当收到心率数据时调用
        private static void NotificationHandler(GattCharacteristic characteristic, GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] data = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(data);
            
            // 解析心率数据
            // 转换为十六进制字符串并查找 '06' 后的值
            string hexString = BitConverter.ToString(data).Replace("-", "");
            int index = hexString.IndexOf("06");
            
            if (index >= 0 && index + 2 < hexString.Length)
            {
                string valueHex = hexString.Substring(index + 2, 2);
                heartRateValue = Convert.ToInt32(valueHex, 16);
                Console.WriteLine(heartRateValue);
            }
        }

        // 扫描设备并根据名称获取设备
        private static async Task<DeviceInformation> ScanForDeviceAsync(string deviceName)
        {
            // 查找蓝牙 LE 设备
            string selector = BluetoothLEDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);
            
            foreach (var device in devices)
            {
                // 使用模糊匹配
                if (!string.IsNullOrEmpty(device.Name) && device.Name.Contains(deviceName))
                {
                    return device;
                }
            }
            
            return null;
        }

        // 查找描述为"Heart Rate Measurement"的特征
        private static async Task<GattCharacteristic> FindHeartRateMeasurementCharacteristic(
            BluetoothLEDevice device)
        {
            var servicesResult = await device.GetGattServicesAsync();
            
            if (servicesResult.Status == GattCommunicationStatus.Success)
            {
                foreach (var service in servicesResult.Services)
                {
                    var characteristicsResult = await service.GetCharacteristicsAsync();
                    
                    if (characteristicsResult.Status == GattCommunicationStatus.Success)
                    {
                        foreach (var characteristic in characteristicsResult.Characteristics)
                        {
                            // 心率测量特征的标准 UUID
                            // 也可以通过 UserDescription 来匹配
                            if (characteristic.Uuid == GattCharacteristicUuids.HeartRateMeasurement)
                            {
                                return characteristic;
                            }
                        }
                    }
                }
            }
            
            return null;
        }

        // 主函数
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting scan...");
            
            // 扫描设备
            var deviceInfo = await ScanForDeviceAsync(DeviceName);
            
            if (deviceInfo == null)
            {
                Console.WriteLine($"No device found containing '{DeviceName}' in its name.");
                return;
            }
            
            Console.WriteLine($"Found device: {deviceInfo.Name} ({deviceInfo.Id})");
            
            // 连接到设备
            Console.WriteLine("Connecting to device...");
            var device = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
            
            if (device == null)
            {
                Console.WriteLine("Failed to connect to device.");
                return;
            }
            
            Console.WriteLine("Connected");
            
            // 初始化断开连接事件
            disconnectedEvent = new TaskCompletionSource<bool>();
            
            // 设置断开连接回调
            device.ConnectionStatusChanged += (sender, args) =>
            {
                if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
                {
                    Console.WriteLine("Disconnected callback called!");
                    disconnectedEvent.TrySetResult(true);
                }
            };
            
            try
            {
                // 查找心率测量特征
                var heartRateCharacteristic = await FindHeartRateMeasurementCharacteristic(device);
                
                if (heartRateCharacteristic != null)
                {
                    // 配置特征以接收通知
                    var status = await heartRateCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    
                    if (status == GattCommunicationStatus.Success)
                    {
                        // 订阅通知事件
                        heartRateCharacteristic.ValueChanged += NotificationHandler;
                        
                        Console.WriteLine("Subscribed to heart rate notifications. Waiting for data...");
                        
                        // 等待断开连接事件
                        await disconnectedEvent.Task;
                    }
                    else
                    {
                        Console.WriteLine("Failed to subscribe to notifications.");
                    }
                }
                else
                {
                    Console.WriteLine("Heart Rate Measurement characteristic not found.");
                }
            }
            finally
            {
                device?.Dispose();
            }
        }
    }
}