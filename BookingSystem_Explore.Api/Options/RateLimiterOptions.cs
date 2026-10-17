namespace BookingSystem_Explore.Api.Options
{
    public class RateLimiterOptions
    {
        public int TokenLimit { get; set; } = 20;
        public int TokensPerPeriod { get; set; } = 20;
        public int ReplenishmentMinutes { get; set; } = 1;
        public int QueueLimit { get; set; } = 0;
    }
}
