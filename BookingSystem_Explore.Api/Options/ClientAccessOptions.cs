namespace BookingSystem_Explore.Api.Options
{
    public class ClientAccessOptions
    {
        public List<ClientDefinition> Clients { get; set; } = new();
        public JwtClientOptions Jwt { get; set; } = new();
        public bool RequireClientCertificate { get; set; }
    }

    public class ClientDefinition
    {
        public string ClientId { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = [];
        public string[] AllowedCertificateThumbprints { get; set; } = [];
        public ClientRateLimitOptions RateLimit { get; set; } = new();
    }

    public class JwtClientOptions
    {
        public string Issuer { get; set; } = "BookingSystem_Explore.Ui";
        public string Audience { get; set; } = "BookingSystem_Explore.Api";
        public string SigningKey { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 30;
    }

    public class ClientRateLimitOptions
    {
        public int TokenLimit { get; set; } = 20;
        public int TokensPerPeriod { get; set; } = 20;
        public int ReplenishmentMinutes { get; set; } = 1;
        public int QueueLimit { get; set; } = 0;
    }
}
