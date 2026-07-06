namespace DisBlox.Services;

public interface IBotConnectionService
{
    Task<bool> ConnectAsync(string robloxUsername, CancellationToken cancellationToken = default);
}
