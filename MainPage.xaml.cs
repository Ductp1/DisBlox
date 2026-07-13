using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DisBlox.Services;
using DisBlox.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DisBlox;

/// <summary>
/// The main content page displayed inside the application window.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; } = new();

    private bool _suppressNextTextChanged;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressNextTextChanged)
        {
            _suppressNextTextChanged = false;
            return;
        }

        await ViewModel.OnUsernameTextChangedAsync(((TextBox)sender).Text);
    }

    private void SuggestionsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is RobloxUserSuggestion suggestion)
        {
            _suppressNextTextChanged = true;
            ViewModel.SelectSuggestion(suggestion);
        }
    }

    private void RemoveGuildButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string guildId })
        {
            ViewModel.RemoveGuildCommand.Execute(guildId);
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SetupPage));
    }
}
