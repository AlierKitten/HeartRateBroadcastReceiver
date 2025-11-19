using System.ComponentModel;
using System.Runtime.CompilerServices;
using HeartRateBroadcastReceiver.Views.Pages;

namespace HeartRateBroadcastReceiver.ViewModels.Pages;

public class HomePageViewModel : INotifyPropertyChanged
{
    private string _selectedDevice = "Xiaomi";
    private string _heartRate = "---";
    private string _connectionStatus = "未监听";

    public string SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            _selectedDevice = value;
            OnPropertyChanged();
        }
    }

    public string HeartRate
    {
        get => _heartRate;
        set
        {
            _heartRate = value;
            OnPropertyChanged();

            // 更新图表数据
            UpdateChartData();
        }
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set
        {
            _connectionStatus = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdateChartData()
    {
        // 如果心率是有效数字，则将其添加到图表数据中
        if (int.TryParse(_heartRate, out int heartRateValue))
        {
            var dataPage = App.Services.GetService(typeof(DataPage)) as DataPage;
            if (dataPage?.DataContext is DataPageViewModel dataPageViewModel)
            {
                dataPageViewModel.AddHeartRateData(heartRateValue);
            }
        }
    }
}