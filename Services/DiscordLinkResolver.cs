namespace DisBlox.Services;

public sealed record GuildLookupProgress(string GuildId, int CompletedGuilds, int TotalGuilds, string CurrentService);

public sealed record ResolveResult(bool Found, string? DiscordUserId, string? GuildId, string? Source, string? FailureReason);

public sealed class DiscordLinkResolver(
    IRobloxUserIdLookupService userIdLookup,
    IDiscordLinkLookupService? bloxLink,
    IDiscordLinkLookupService? rover)
{
    public async Task<ResolveResult> ResolveAsync(
        string robloxUsername,
        long? knownRobloxUserId,
        IReadOnlyList<string> guildIds,
        IProgress<GuildLookupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        long robloxUserId;

        if (knownRobloxUserId is not null)
        {
            robloxUserId = knownRobloxUserId.Value;
        }
        else
        {
            long? lookedUpId;
            try
            {
                lookedUpId = await userIdLookup.GetUserIdAsync(robloxUsername, cancellationToken);
            }
            catch (Exception ex)
            {
                return new ResolveResult(false, null, null, null, $"Roblox username lookup failed: {ex.Message}");
            }

            if (lookedUpId is null)
                return new ResolveResult(false, null, null, null, $"Roblox user '{robloxUsername}' does not exist");

            robloxUserId = lookedUpId.Value;
        }

        var failureReasons = new List<string>();

        for (var i = 0; i < guildIds.Count; i++)
        {
            var guildId = guildIds[i];

            if (bloxLink is not null)
            {
                progress?.Report(new GuildLookupProgress(guildId, i, guildIds.Count, bloxLink.Name));
                var result = await SafeLookupAsync(bloxLink, guildId, robloxUserId, cancellationToken);
                if (result.Found)
                    return new ResolveResult(true, result.DiscordUserIds[0], guildId, bloxLink.Name, null);
                failureReasons.Add($"[{guildId}] {bloxLink.Name}: {result.FailureReason}");
            }

            if (rover is not null)
            {
                progress?.Report(new GuildLookupProgress(guildId, i, guildIds.Count, rover.Name));
                var result = await SafeLookupAsync(rover, guildId, robloxUserId, cancellationToken);
                if (result.Found)
                    return new ResolveResult(true, result.DiscordUserIds[0], guildId, rover.Name, null);
                failureReasons.Add($"[{guildId}] {rover.Name}: {result.FailureReason}");
            }

            progress?.Report(new GuildLookupProgress(guildId, i + 1, guildIds.Count, ""));
        }

        return new ResolveResult(false, null, null, null, string.Join("; ", failureReasons));
    }

    private static async Task<LookupResult> SafeLookupAsync(
        IDiscordLinkLookupService service,
        string guildId,
        long robloxUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await service.LookupAsync(guildId, robloxUserId, cancellationToken);
        }
        catch (Exception ex)
        {
            return new LookupResult(false, [], $"exception: {ex.Message}");
        }
    }
}
