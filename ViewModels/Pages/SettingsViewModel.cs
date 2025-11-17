using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace HeartRateBroadcastReceiver.ViewModels.Pages;
public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized = false;

    [ObservableProperty]
    private string _appVersion = String.Empty;

    public Task OnNavigatedToAsync()
    {
        if (!_isInitialized)
            InitializeViewModel();

        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    private void InitializeViewModel()
    {

        ApplicationThemeManager.Apply(ApplicationTheme.Light);

        AppVersion = $"Version {GetAssemblyVersion()}";

        _isInitialized = true;
    }

    private string GetAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? String.Empty;
    }
}
