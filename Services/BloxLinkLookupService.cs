using System.Text.Json;

namespace DisBlox.Services;

public sealed class BloxLinkLookupService : IDiscordLinkLookupService
{
    private readonly HttpClient _http;

    public string Name => "BloxLink";

    public BloxLinkLookupService(string apiKey)
    {
        _http = new HttpClient { BaseAddress = new Uri("https://api.blox.link/v4/public/") };
        _http.DefaultRequestHeaders.Add("Authorization", apiKey);
    }

    public async Task<LookupResult> LookupAsync(string guildId, long robloxUserId, CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync($"guilds/{guildId}/roblox-to-discord/{robloxUserId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var reason = await ReadErrorReasonAsync(response, cancellationToken);
            return new LookupResult(false, [], reason);
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var discordIds = new List<string>();

        if (doc.RootElement.TryGetProperty("discordIDs", out var idsElement))
        {
            foreach (var idItem in idsElement.EnumerateArray())
                discordIds.Add(idItem.GetString()!);
        }

        return discordIds.Count > 0
            ? new LookupResult(true, discordIds, null)
            : new LookupResult(false, [], "not linked in this server");
    }

    private static async Task<string> ReadErrorReasonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
            var message = doc.RootElement.TryGetProperty("error", out var err) ? err.GetString() : null;
            return message ?? $"HTTP {(int)response.StatusCode}";
        }
        catch (JsonException)
        {
            return $"HTTP {(int)response.StatusCode}";
        }
    }
}
