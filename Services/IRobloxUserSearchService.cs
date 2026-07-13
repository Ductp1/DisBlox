namespace DisBlox.Services;

public interface IRobloxUserSearchService
{
    Task<IReadOnlyList<RobloxUserSuggestion>> SearchAsync(string keyword, CancellationToken cancellationToken = default);
}
