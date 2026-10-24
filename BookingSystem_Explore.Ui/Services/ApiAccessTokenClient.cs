using System.Net.Http.Json;

namespace BookingSystem_Explore.Ui.Services;

public sealed class ApiAccessTokenClient
{
    private readonly HttpClient _httpClient;
    private readonly ApiClientSettings _settings;
    private string? _accessToken;
    private DateTime _expiresAtUtc;

    public ApiAccessTokenClient(HttpClient httpClient, ApiClientSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && _expiresAtUtc > DateTime.UtcNow.AddMinutes(1))
        {
            return _accessToken;
        }

        using var response = await _httpClient.PostAsJsonAsync(
            "api/v1/auth/token",
            new TokenRequest(_settings.ClientId),
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("API returned an empty token response.");

        _accessToken = token.AccessToken;
        _expiresAtUtc = token.ExpiresAtUtc;
        return _accessToken;
    }

    private sealed record TokenRequest(string ClientId);
    private sealed record TokenResponse(string AccessToken, DateTime ExpiresAtUtc);
}
