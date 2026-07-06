namespace DisBlox.Services;

// Placeholder until the server-hosted bot exposes a real setup API.
public sealed class BotConnectionService : IBotConnectionService
{
    public async Task<bool> ConnectAsync(string robloxUsername, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000, cancellationToken);
        return !string.IsNullOrWhiteSpace(robloxUsername);
    }
}
