using BookingSystem_Explore.Ui.Models;
using System.Text;
using System.Text.Json;

namespace BookingSystem_Explore.Ui.Services;

public class BookingApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly ApiClientSettings _settings;
    private readonly PhoneEncryptionClient _phoneEncryptionClient;
    private readonly JwtTokenClient _jwtTokenClient;

    public BookingApiClient(HttpClient httpClient, ApiClientSettings settings, PhoneEncryptionClient phoneEncryptionClient, JwtTokenClient jwtTokenClient)
    {
        _httpClient = httpClient;
        _settings = settings;
        _phoneEncryptionClient = phoneEncryptionClient;
        _jwtTokenClient = jwtTokenClient;
    }

    public async Task<List<BranchResponse>> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<BranchResponse>>("api/v1/branches", cancellationToken) ?? [];
    }

    public async Task<List<BranchServiceResponse>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<BranchServiceResponse>>($"api/v1/branches/{branchId}/services", cancellationToken) ?? [];
    }

    public async Task<List<AppointmentSlotResponse>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<AppointmentSlotResponse>>($"api/v1/appointment-slots?branchServiceId={branchServiceId}&date={date:yyyy-MM-dd}", cancellationToken) ?? [];
    }

    public async Task<AppointmentCreatedResponse> BookAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        request.Customer.PhoneNumber = _phoneEncryptionClient.Encrypt(request.Customer.PhoneNumber);
        var requestJson = JsonSerializer.Serialize(request, JsonOptions);
        using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        using var message = CreateRequest(HttpMethod.Post, "api/v1/appointments", "write");
        message.Content = content;
        var response = await _httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await DeserializeResponseAsync<AppointmentCreatedResponse>(response, cancellationToken);
        if (payload is null)
        {
            throw new InvalidOperationException("API returned an empty booking response.");
        }

        return payload;
    }

    private async Task<T?> GetAsync<T>(string route, CancellationToken cancellationToken)
    {
        using var message = CreateRequest(HttpMethod.Get, route, "read");
        var response = await _httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string route, string scope)
    {
        var message = new HttpRequestMessage(method, route);
        message.Headers.Add("X-Client-Id", _settings.ClientId);
        message.Headers.Add("X-Client-Secret", _settings.ClientSecret);
        message.Headers.Add("X-Session-Id", Guid.NewGuid().ToString("N"));
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtTokenClient.CreateToken(scope));
        return message;
    }

    private static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
    }
}
