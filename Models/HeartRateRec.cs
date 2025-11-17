using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

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

        public HeartRateMonitor(string deviceName, Action<int> heartRateCallback, Action<string> connectionCallback = null)
        {
            this.deviceName = deviceName;
            this.heartRateUpdateCallback = heartRateCallback;
            this.connectionResultCallback = connectionCallback;
            
            // 初始化广播监听器
            advertisementWatcher = new BluetoothLEAdvertisementWatcher();
            advertisementWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            
            // 设置事件处理
            advertisementWatcher.Received += OnAdvertisementReceived;
        }

        // 处理收到的广播数据
        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // 检查设备名称是否匹配
            if (!string.IsNullOrEmpty(args.Advertisement.LocalName) && 
                (args.Advertisement.LocalName.Contains(deviceName, StringComparison.OrdinalIgnoreCase) || 
                 string.IsNullOrEmpty(deviceName)))
            {
                LogAndCallback($"发现目标设备广播: {args.Advertisement.LocalName}, RSSI: {args.RawSignalStrengthInDBm}");
                
                // 解析心率数据（如果包含在广播中）
                ParseHeartRateData(args.Advertisement, args.RawSignalStrengthInDBm);
            }
        }
        
        // 解析心率数据
        private void ParseHeartRateData(BluetoothLEAdvertisement advertisement, short rssi)
        {
            // 在广播数据中查找心率服务数据
            foreach (var dataSection in advertisement.DataSections)
            {
                var reader = DataReader.FromBuffer(dataSection.Data);
                byte[] data = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(data);
                
                // 输出所有数据段用于调试
                string hexString = BitConverter.ToString(data).Replace("-", "");
                LogAndCallback($"数据段类型: {dataSection.DataType}, 数据: {hexString}");
            }
        }

        // 开始监听广播
        public void StartListening()
        {
            LogAndCallback("开始监听BLE广播...");
            advertisementWatcher.Start();
        }
        
        // 停止监听广播
        public void StopListening()
        {
            LogAndCallback("停止监听BLE广播");
            advertisementWatcher.Stop();
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