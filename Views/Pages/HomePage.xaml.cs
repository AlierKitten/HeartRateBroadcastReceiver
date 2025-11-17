using HeartRateBroadcast;
using HeartRateBroadcastReceiver.ViewModels.Pages;
using System.Windows.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace HeartRateBroadcastReceiver.Views.Pages;
public partial class HomePage : Page
{
    private HomePageViewModel _viewModel;
    private HeartRateMonitor _heartRateMonitor;
    private bool _isListening = false;

    public HomePage()
    {
        InitializeComponent();
        _viewModel = new HomePageViewModel();
        DataContext = _viewModel;
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        // 获取选中的设备名称
        var selectedItem = DeviceComboBox.SelectedItem as ComboBoxItem;
        if (selectedItem != null)
        {
            string deviceName = selectedItem.Content.ToString();
            _viewModel.SelectedDevice = deviceName;
            _viewModel.ConnectionStatus = "监听中...";

            // 创建心率监控器实例
            _heartRateMonitor = new HeartRateMonitor(deviceName, OnHeartRateUpdated, OnConnectionStatusChanged);
            
            // 开始监听广播
            _heartRateMonitor.StartListening();
            _isListening = true;
            ConnectButton.IsEnabled = false;
            DisconnectButton.IsEnabled = true;
        }
    }

    private void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_heartRateMonitor != null && _isListening)
        {
            _heartRateMonitor.StopListening();
            _isListening = false;
            _viewModel.ConnectionStatus = "已停止监听";
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
        }
    }

    private void OnHeartRateUpdated(int heartRate)
    {
        Dispatcher.Invoke(() =>
        {
            _viewModel.HeartRate = heartRate.ToString();
        });
    }
    
    private void OnConnectionStatusChanged(string status)
    {
        Dispatcher.Invoke(() =>
        {
            _viewModel.ConnectionStatus = status;
        });
    }
}