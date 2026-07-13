using System.Net.Http.Json;
using System.Text.Json;

namespace DisBlox.Services;

public sealed class RobloxUserIdLookupService : IRobloxUserIdLookupService
{
    private static readonly HttpClient Http = new() { BaseAddress = new Uri("https://users.roblox.com/") };

    public async Task<long?> GetUserIdAsync(string username, CancellationToken cancellationToken = default)
    {
        var requestBody = new { usernames = new[] { username }, excludeBannedUsers = false };
        using var response = await Http.PostAsJsonAsync("v1/usernames/users", requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Roblox username lookup returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var data = doc.RootElement.GetProperty("data");
        if (data.GetArrayLength() == 0)
            return null;

        return data[0].GetProperty("id").GetInt64();
    }
}
