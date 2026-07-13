using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DisBlox.ViewModels;

namespace DisBlox;

public sealed partial class SetupPage : Page
{
    public SetupViewModel ViewModel { get; } = new();

    public SetupPage()
    {
        InitializeComponent();
        RoverKeyBox.Password = ViewModel.RoverApiKey;
        BloxLinkKeyBox.Password = ViewModel.BloxLinkApiKey;
        ViewModel.SetupCompleted += () => Frame.Navigate(typeof(MainPage));
    }

    private void RoverKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.RoverApiKey = RoverKeyBox.Password;
    }

    private void BloxLinkKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.BloxLinkApiKey = BloxLinkKeyBox.Password;
    }
}
