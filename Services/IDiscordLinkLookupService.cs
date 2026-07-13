namespace DisBlox.Services;

public interface IDiscordLinkLookupService
{
    string Name { get; }

    Task<LookupResult> LookupAsync(string guildId, long robloxUserId, CancellationToken cancellationToken = default);
}
