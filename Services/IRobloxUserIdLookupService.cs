namespace DisBlox.Services;

public interface IRobloxUserIdLookupService
{
    Task<long?> GetUserIdAsync(string username, CancellationToken cancellationToken = default);
}
