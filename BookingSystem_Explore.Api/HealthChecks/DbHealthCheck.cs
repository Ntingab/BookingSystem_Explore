using Microsoft.Extensions.Diagnostics.HealthChecks;
using BookingSystem_Explore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem_Explore.Api.HealthChecks
{
    public class DbHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _sp;
        public DbHealthCheck(IServiceProvider sp)
        {
            _sp = sp;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppointmentBookingDbContext>();
                var canConnect = await db.Database.CanConnectAsync(cancellationToken);
                return canConnect ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Cannot connect to database");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Exception checking database health", ex);
            }
        }
    }
}
