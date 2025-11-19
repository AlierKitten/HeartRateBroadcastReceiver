using System.Windows.Controls;
using HeartRateBroadcastReceiver.ViewModels.Pages;

namespace HeartRateBroadcastReceiver.Views.Pages;

public partial class DataPage : Page
{
    public DataPage(DataPageViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}