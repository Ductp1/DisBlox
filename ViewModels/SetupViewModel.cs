using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DisBlox.Services;

namespace DisBlox.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    private readonly ICredentialStore _credentialStore;

    [ObservableProperty]
    public partial string RoverApiKey { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BloxLinkApiKey { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public event Action? SetupCompleted;

    public SetupViewModel() : this(new WindowsCredentialStore())
    {
    }

    public SetupViewModel(ICredentialStore credentialStore)
    {
        _credentialStore = credentialStore;
        RoverApiKey = credentialStore.Get(CredentialKeys.RoverApiKey) ?? string.Empty;
        BloxLinkApiKey = credentialStore.Get(CredentialKeys.BloxLinkApiKey) ?? string.Empty;
    }

    [RelayCommand]
    private void Continue()
    {
        if (string.IsNullOrWhiteSpace(RoverApiKey) && string.IsNullOrWhiteSpace(BloxLinkApiKey))
        {
            ErrorMessage = "Enter at least one API key (RoVer or BloxLink) to continue.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(RoverApiKey))
            _credentialStore.Save(CredentialKeys.RoverApiKey, RoverApiKey);

        if (!string.IsNullOrWhiteSpace(BloxLinkApiKey))
            _credentialStore.Save(CredentialKeys.BloxLinkApiKey, BloxLinkApiKey);

        SetupCompleted?.Invoke();
    }
}
