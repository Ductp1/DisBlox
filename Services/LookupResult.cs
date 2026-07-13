namespace DisBlox.Services;

public sealed record LookupResult(bool Found, IReadOnlyList<string> DiscordUserIds, string? FailureReason);
