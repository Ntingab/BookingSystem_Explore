namespace BookingSystem_Explore.Ui.Services;

public class ApiClientSettings
{
    public string BaseUrl { get; set; } = "https://localhost:7086/";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string PhoneEncryptionKey { get; set; } = string.Empty;
    public string JwtSigningKey { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = "BookingSystem_Explore.Ui";
    public string JwtAudience { get; set; } = "BookingSystem_Explore.Api";
    public int JwtExpiryMinutes { get; set; } = 30;
}
