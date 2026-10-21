using BookingSystem_Explore.Ui.Models;
using System.Text;
using System.Text.Json;

namespace BookingSystem_Explore.Ui.Services;

public class BookingApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<BranchResponse>> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<BranchResponse>>("api/v1/branches", cancellationToken) ?? [];
    }

    public async Task<List<ServiceResponse>> GetBranchServicesAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<ServiceResponse>>($"api/v1/branches/{branchId}/services", cancellationToken) ?? [];
    }

    public async Task<List<AppointmentSlotResponse>> GetAvailableSlotsAsync(Guid branchServiceId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await GetAsync<List<AppointmentSlotResponse>>($"api/v1/appointment-slots?branchServiceId={branchServiceId}&date={date:yyyy-MM-dd}", cancellationToken) ?? [];
    }

    public async Task<AppointmentCreatedResponse> BookAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var requestJson = JsonSerializer.Serialize(request, JsonOptions);
        using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("api/v1/appointments", content, cancellationToken);
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
        var response = await httpClient.GetAsync(route, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await DeserializeResponseAsync<T>(response, cancellationToken);
    }

    private static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
    }
}
