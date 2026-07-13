using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DisBlox.Services;

namespace DisBlox.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IRobloxUserSearchService _userSearchService;
    private readonly DiscordLinkResolver _resolver;
    private CancellationTokenSource? _searchCts;

    [ObservableProperty]
    public partial string RobloxUsername { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Enter a Roblox username and search.";

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSearching { get; set; }

    [ObservableProperty]
    public partial double SearchProgress { get; set; }

    [ObservableProperty]
    public partial string NewGuildId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ChosenUsername { get; set; } = string.Empty;

    private long? _resolvedRobloxUserId;

    public ObservableCollection<RobloxUserSuggestion> UsernameSuggestions { get; } = [];

    public ObservableCollection<string> GuildIds { get; } = [];

    public MainPageViewModel() : this(new RobloxUserSearchService(), BuildResolver())
    {
    }

    public MainPageViewModel(IRobloxUserSearchService userSearchService, DiscordLinkResolver resolver)
    {
        _userSearchService = userSearchService;
        _resolver = resolver;
    }

    private static DiscordLinkResolver BuildResolver()
    {
        var store = new WindowsCredentialStore();
        var roverKey = store.Get(CredentialKeys.RoverApiKey);
        var bloxLinkKey = store.Get(CredentialKeys.BloxLinkApiKey);

        IDiscordLinkLookupService? bloxLink = string.IsNullOrEmpty(bloxLinkKey) ? null : new BloxLinkLookupService(bloxLinkKey);
        IDiscordLinkLookupService? rover = string.IsNullOrEmpty(roverKey) ? null : new RoverLookupService(roverKey);

        return new DiscordLinkResolver(new RobloxUserIdLookupService(), bloxLink, rover);
    }

    public async Task OnUsernameTextChangedAsync(string text)
    {
        _searchCts?.Cancel();
        var cts = new CancellationTokenSource();
        _searchCts = cts;

        try
        {
            await Task.Delay(250, cts.Token);
            ErrorMessage = string.Empty;
            var results = await _userSearchService.SearchAsync(text, cts.Token);

            UsernameSuggestions.Clear();
            foreach (var result in results)
                UsernameSuggestions.Add(result);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex) when (!cts.IsCancellationRequested)
        {
            ErrorMessage = $"Username search failed: {ex.Message}";
        }
    }

    public void SelectSuggestion(RobloxUserSuggestion suggestion)
    {
        _resolvedRobloxUserId = suggestion.UserId;
        ChosenUsername = suggestion.Name;
        RobloxUsername = suggestion.Name;
        UsernameSuggestions.Clear();
    }

    [RelayCommand]
    private void AddGuild()
    {
        var guildId = NewGuildId.Trim();
        if (string.IsNullOrWhiteSpace(guildId) || GuildIds.Contains(guildId))
            return;

        GuildIds.Add(guildId);
        NewGuildId = string.Empty;
        SearchCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void RemoveGuild(string guildId)
    {
        GuildIds.Remove(guildId);
        SearchCommand.NotifyCanExecuteChanged();
    }

    private bool CanSearch() => !IsSearching && !string.IsNullOrWhiteSpace(RobloxUsername) && GuildIds.Count > 0;

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync()
    {
        IsSearching = true;
        SearchProgress = 0;
        StatusMessage = $"Searching for {RobloxUsername}...";
        ErrorMessage = string.Empty;
        SearchCommand.NotifyCanExecuteChanged();

        var progress = new Progress<GuildLookupProgress>(p =>
        {
            SearchProgress = p.TotalGuilds == 0 ? 0 : (double)p.CompletedGuilds / p.TotalGuilds * 100;
            if (!string.IsNullOrEmpty(p.CurrentService))
                StatusMessage = $"Checking guild {p.GuildId} via {p.CurrentService}...";
        });

        try
        {
            var result = await _resolver.ResolveAsync(RobloxUsername, _resolvedRobloxUserId, [.. GuildIds], progress);
            if (result.Found)
            {
                StatusMessage = $"Found! Discord user {result.DiscordUserId} (via {result.Source} in guild {result.GuildId})";
                ErrorMessage = string.Empty;
            }
            else
            {
                StatusMessage = string.Empty;
                ErrorMessage = result.FailureReason ?? "Search failed for an unknown reason.";
            }
            SearchProgress = 100;
        }
        catch (Exception ex)
        {
            StatusMessage = string.Empty;
            ErrorMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
            SearchCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnRobloxUsernameChanged(string value)
    {
        if (!string.Equals(value, ChosenUsername, StringComparison.OrdinalIgnoreCase))
        {
            ChosenUsername = string.Empty;
            _resolvedRobloxUserId = null;
        }

        SearchCommand.NotifyCanExecuteChanged();
    }
}
