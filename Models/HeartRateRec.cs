using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.Collections.Concurrent;

namespace HeartRateBroadcast
{
    public class HeartRateMonitor
    {
        // 设备名称
        private string deviceName;
        
        // 心率值
        private static int heartRateValue = 0;
        
        // 心率更新回调
        private Action<int> heartRateUpdateCallback;
        
        // 连接结果回调
        private Action<string> connectionResultCallback;
        
        // BLE广播监听器
        private BluetoothLEAdvertisementWatcher advertisementWatcher;
        
        // 存储已连接的设备
        private readonly ConcurrentDictionary<ulong, BluetoothLEDevice> connectedDevices = new();
        
        // 存储已订阅通知的设备地址
        private readonly ConcurrentDictionary<ulong, GattCharacteristic> subscribedCharacteristics = new();

        public HeartRateMonitor(string deviceName, Action<int> heartRateCallback, Action<string> connectionCallback = null)
        {
            this.deviceName = deviceName;
            this.heartRateUpdateCallback = heartRateCallback;
            this.connectionResultCallback = connectionCallback;
            
            // 初始化广播监听器
            advertisementWatcher = new BluetoothLEAdvertisementWatcher();
            advertisementWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            
            // 添加心率服务过滤
            advertisementWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(GattServiceUuids.HeartRate);
            
            // 设置事件处理
            advertisementWatcher.Received += OnAdvertisementReceived;
        }

        // 处理收到的广播数据
        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            try
            {
                // 检查设备名称是否匹配
                if (!string.IsNullOrEmpty(args.Advertisement.LocalName) && 
                    (args.Advertisement.LocalName.Contains(deviceName, StringComparison.OrdinalIgnoreCase) || 
                     string.IsNullOrEmpty(deviceName)))
                {
                    LogAndCallback($"发现目标设备广播: {args.Advertisement.LocalName}, RSSI: {args.RawSignalStrengthInDBm}");
                    
                    // 检查是否已经订阅了该设备的心率通知
                    if (subscribedCharacteristics.ContainsKey(args.BluetoothAddress))
                        return;
                    
                    // 获取蓝牙设备
                    var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
                    if (device == null) 
                        return;
                    
                    connectedDevices.TryAdd(device.BluetoothAddress, device);
                    
                    // 获取心率服务
                    var hrServices = await device.GetGattServicesForUuidAsync(GattServiceUuids.HeartRate);
                    if (hrServices.Status != GattCommunicationStatus.Success || hrServices.Services.Count == 0)
                    {
                        LogAndCallback($"无法获取 {device.Name} 的心率服务");
                        return;
                    }
                    
                    var hrService = hrServices.Services[0];

                    // 获取心率特征值
                    var hrCharacteristics = await hrService.GetCharacteristicsForUuidAsync(GattCharacteristicUuids.HeartRateMeasurement);
                    if (hrCharacteristics.Status != GattCommunicationStatus.Success || hrCharacteristics.Characteristics.Count == 0)
                    {
                        LogAndCallback($"无法获取 {device.Name} 的心率特征值");
                        return;
                    }
                    
                    var hrCharacteristic = hrCharacteristics.Characteristics[0];

                    // 启用特征值通知
                    hrCharacteristic.ValueChanged += HeartRateValueChanged;
                    var status = await hrCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    
                    if (status == GattCommunicationStatus.Success)
                    {
                        // 记录已订阅通知的特征
                        subscribedCharacteristics.TryAdd(device.BluetoothAddress, hrCharacteristic);
                        LogAndCallback($"已成功订阅 {device.Name} 的心率数据通知");
                    }
                    else
                    {
                        LogAndCallback($"订阅 {device.Name} 心率数据通知失败: {status}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogAndCallback($"处理设备广播时出错: {ex.Message}");
            }
        }
        
        private void HeartRateValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                using var reader = DataReader.FromBuffer(args.CharacteristicValue);
                var heartRate = ParseHeartRateValue(reader);
                heartRateUpdateCallback?.Invoke(heartRate);
                LogAndCallback($"心率更新: {heartRate} BPM");
            }
            catch (Exception ex)
            {
                LogAndCallback($"解析心率数据时出错: {ex.Message}");
            }
        }
        
        private static int ParseHeartRateValue(DataReader reader)
        {
            reader.ByteOrder = ByteOrder.LittleEndian;

            // 读取标志位
            byte flags = reader.ReadByte();
            bool is16bit = (flags & 0x01) != 0;

            // 根据标志位确定心率值格式并读取
            return is16bit ? reader.ReadUInt16() : reader.ReadByte();
        }

        // 开始监听广播
        public void StartListening()
        {
            LogAndCallback("开始监听BLE广播...");
            // 清除之前的订阅记录，允许重新订阅
            subscribedCharacteristics.Clear();
            advertisementWatcher.Start();
        }
        
        // 停止监听广播
        public void StopListening()
        {
            LogAndCallback("停止监听BLE广播");
            advertisementWatcher.Stop();
            
            // 清理已订阅的特征通知
            foreach (var kvp in subscribedCharacteristics)
            {
                var characteristic = kvp.Value;
                if (characteristic != null)
                {
                    // 尝试取消订阅通知
                    _ = characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.None);
                    characteristic.ValueChanged -= HeartRateValueChanged;
                }
            }
            subscribedCharacteristics.Clear();
            
            // 清理已连接的设备
            foreach (var device in connectedDevices.Values)
            {
                device.Dispose();
            }
            connectedDevices.Clear();
        }
        
        // 日志和回调方法
        private void LogAndCallback(string message)
        {
            Console.WriteLine(message);
            connectionResultCallback?.Invoke(message);
        }
        
        // 析构函数，确保资源释放
        ~HeartRateMonitor()
        {
            StopListening();
        }
    }
}