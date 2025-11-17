using System.ComponentModel;
using System.Runtime.CompilerServices;
using Wpf.Ui.Abstractions.Controls;

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
}