using BookingSystem_Explore.Ui.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookingSystem_Explore.Ui.Services;

public class BookingApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly ApiClientSettings _settings;
    private readonly ApiAccessTokenClient _accessTokenClient;

    public BookingApiClient(HttpClient httpClient, ApiClientSettings settings, ApiAccessTokenClient accessTokenClient)
    {
        _httpClient = httpClient;
        _settings = settings;
        _accessTokenClient = accessTokenClient;
    }

    public async Task<List<ServiceResponse>> GetServicesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<ServiceResponse>>("api/v1/services", cancellationToken) ?? [];
    }

    public Task<BranchResponse> AddBranchAsync(BranchRequest request, CancellationToken cancellationToken = default)
        => SendAsync<BranchResponse>(HttpMethod.Post, "api/v1/branches", request, cancellationToken);

    public Task<BranchResponse> UpdateBranchAsync(Guid branchId, BranchRequest request, CancellationToken cancellationToken = default)
        => SendAsync<BranchResponse>(HttpMethod.Put, $"api/v1/branches/{branchId}", request, cancellationToken);

    public Task<BranchServiceResponse> AddBranchServiceAsync(Guid branchId, CreateBranchServiceRequest request, CancellationToken cancellationToken = default)
        => SendAsync<BranchServiceResponse>(HttpMethod.Post, $"api/v1/branches/{branchId}/services", request, cancellationToken);

    public Task<BranchServiceResponse> UpdateBranchServiceAsync(Guid branchId, Guid branchServiceId, UpdateBranchServiceRequest request, CancellationToken cancellationToken = default)
        => SendAsync<BranchServiceResponse>(HttpMethod.Put, $"api/v1/branches/{branchId}/services/{branchServiceId}", request, cancellationToken);

    public async Task<List<BranchResponse>> GetBranchesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<BranchResponse>>($"api/v1/branches?includeInactive={includeInactive}", cancellationToken) ?? [];
    }

    public async Task<List<BranchServiceResponse>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<BranchServiceResponse>>($"api/v1/branches/{branchId}/services", cancellationToken) ?? [];
    }

    public async Task<List<AppointmentSlotResponse>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<AppointmentSlotResponse>>($"api/v1/appointment-slots?branchServiceId={branchServiceId}&date={date:yyyy-MM-dd}", cancellationToken) ?? [];
    }

    public Task<AppointmentSlotResponse> AddAppointmentSlotAsync(CreateAppointmentSlotRequest request, CancellationToken cancellationToken = default)
        => SendAsync<AppointmentSlotResponse>(HttpMethod.Post, "api/v1/appointment-slots", request, cancellationToken);

    public Task<AppointmentSlotResponse> UpdateAppointmentSlotAsync(Guid appointmentSlotId, UpdateAppointmentSlotRequest request, CancellationToken cancellationToken = default)
        => SendAsync<AppointmentSlotResponse>(HttpMethod.Put, $"api/v1/appointment-slots/{appointmentSlotId}", request, cancellationToken);

    public Task<AppointmentCreatedResponse> BookAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
        => SendAsync<AppointmentCreatedResponse>(HttpMethod.Post, "api/v1/appointments", request, cancellationToken);

    private async Task<T?> GetAsync<T>(string route, CancellationToken cancellationToken)
    {
        using var message = await CreateRequestAsync(HttpMethod.Get, route, cancellationToken);
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string route, object request, CancellationToken cancellationToken)
    {
        using var message = await CreateRequestAsync(method, route, cancellationToken);
        message.Content = JsonContent.Create(request);
        using var response = await _httpClient.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await DeserializeResponseAsync<T>(response, cancellationToken)
            ?? throw new InvalidOperationException("API returned an empty response.");
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string route, CancellationToken cancellationToken)
    {
        var message = new HttpRequestMessage(method, route);
        message.Headers.Add("X-Client-Id", _settings.ClientId);
        message.Headers.Add("X-Session-Id", Guid.NewGuid().ToString("N"));
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            await _accessTokenClient.GetTokenAsync(cancellationToken));
        return message;
    }

    private static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
    }
}
