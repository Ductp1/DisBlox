using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DisBlox.Services;

public sealed class RoverLookupService : IDiscordLinkLookupService
{
    private readonly HttpClient _http;

    public string Name => "RoVer";

    public RoverLookupService(string apiKey)
    {
        _http = new HttpClient { BaseAddress = new Uri("https://registry.rover.link/api/") };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<LookupResult> LookupAsync(string guildId, long robloxUserId, CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync($"guilds/{guildId}/roblox-to-discord/{robloxUserId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds;
            return new LookupResult(false, [], $"rate limited, retry after {retryAfter ?? 10}s");
        }

        if (!response.IsSuccessStatusCode)
        {
            var reason = await ReadErrorReasonAsync(response, cancellationToken);
            return new LookupResult(false, [], reason);
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var discordIds = new List<string>();

        if (doc.RootElement.TryGetProperty("discordUsers", out var discordUsers))
        {
            foreach (var member in discordUsers.EnumerateArray())
            {
                if (member.TryGetProperty("user", out var user) && user.TryGetProperty("id", out var id))
                    discordIds.Add(id.GetString()!);
            }
        }

        return discordIds.Count > 0
            ? new LookupResult(true, discordIds, null)
            : new LookupResult(false, [], "not opted in or not linked in this server");
    }

    private static async Task<string> ReadErrorReasonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentType?.MediaType != "application/json")
            return $"HTTP {(int)response.StatusCode}";

        try
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
            var errorCode = doc.RootElement.TryGetProperty("errorCode", out var ec) ? ec.GetString() : null;
            var message = doc.RootElement.TryGetProperty("message", out var msg) ? msg.GetString() : null;
            return message ?? errorCode ?? $"HTTP {(int)response.StatusCode}";
        }
        catch (JsonException)
        {
            return $"HTTP {(int)response.StatusCode}";
        }
    }
}
