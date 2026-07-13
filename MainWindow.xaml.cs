using Microsoft.UI.Xaml;
using DisBlox.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DisBlox;

/// <summary>
/// The application window. This hosts a Frame that displays pages. Add your
/// UI and logic to MainPage.xaml / MainPage.xaml.cs instead of here so you
/// can use Page features such as navigation events and the Loaded lifecycle.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        var credentialStore = new WindowsCredentialStore();
        var hasAnyApiKey = !string.IsNullOrEmpty(credentialStore.Get(CredentialKeys.RoverApiKey))
            || !string.IsNullOrEmpty(credentialStore.Get(CredentialKeys.BloxLinkApiKey));

        RootFrame.Navigate(hasAnyApiKey ? typeof(MainPage) : typeof(SetupPage));
    }
}
