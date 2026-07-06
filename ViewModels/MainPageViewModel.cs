using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DisBlox.Services;

namespace DisBlox.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IBotConnectionService _botConnectionService;

    [ObservableProperty]
    public partial string RobloxUsername { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Enter your Roblox username to get started.";

    [ObservableProperty]
    public partial bool IsConnecting { get; set; }

    public MainPageViewModel() : this(new BotConnectionService())
    {
    }

    public MainPageViewModel(IBotConnectionService botConnectionService)
    {
        _botConnectionService = botConnectionService;
    }

    private bool CanConnect() => !IsConnecting && !string.IsNullOrWhiteSpace(RobloxUsername);

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        IsConnecting = true;
        StatusMessage = $"Connecting {RobloxUsername} to the bot...";
        ConnectCommand.NotifyCanExecuteChanged();

        try
        {
            var success = await _botConnectionService.ConnectAsync(RobloxUsername);
            StatusMessage = success
                ? $"Connected! {RobloxUsername} is now linked to the bot."
                : "Couldn't connect. Please check the username and try again.";
        }
        finally
        {
            IsConnecting = false;
            ConnectCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnRobloxUsernameChanged(string value)
    {
        ConnectCommand.NotifyCanExecuteChanged();
    }
}
