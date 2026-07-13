using System.Text.Json;

namespace DisBlox.Services;

public sealed class RobloxUserSearchService : IRobloxUserSearchService
{
    private static readonly HttpClient Http = new() { BaseAddress = new Uri("https://users.roblox.com/") };

    public async Task<IReadOnlyList<RobloxUserSuggestion>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 3)
            return Array.Empty<RobloxUserSuggestion>();

        var url = $"v1/users/search?keyword={Uri.EscapeDataString(keyword)}&limit=10";
        using var response = await Http.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Roblox user search returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var results = new List<RobloxUserSuggestion>();
        foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
        {
            results.Add(new RobloxUserSuggestion(
                item.GetProperty("id").GetInt64(),
                item.GetProperty("name").GetString()!,
                item.GetProperty("displayName").GetString()!));
        }
        return results;
    }
}
