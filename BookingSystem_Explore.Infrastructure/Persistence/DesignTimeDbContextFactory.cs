using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BookingSystem_Explore.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppointmentBookingDbContext>
    {
        public AppointmentBookingDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AppointmentBookingDbContext>();

            // Build configuration from possible appsettings locations (startup project or current directory)
            var configBuilder = new ConfigurationBuilder();
            var current = Directory.GetCurrentDirectory();

            // Prefer startup project's appsettings if present
            var apiProjectPath = Path.Combine(current, "BookingSystem_Explore.Api");
            var apiProjectPathUp = Path.Combine(current, "..", "BookingSystem_Explore.Api");
            if (Directory.Exists(apiProjectPath))
            {
                configBuilder.SetBasePath(apiProjectPath);
            }
            else if (Directory.Exists(apiProjectPathUp))
            {
                configBuilder.SetBasePath(Path.GetFullPath(apiProjectPathUp));
            }
            else
            {
                configBuilder.SetBasePath(current);
            }

            configBuilder
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var configuration = configBuilder.Build();

            // Read connection string from configuration, then environment variable, then fallback
            var conn = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(conn))
            {
                conn = Environment.GetEnvironmentVariable("DefaultConnection");
            }

            if (string.IsNullOrWhiteSpace(conn))
            {
                throw new InvalidOperationException("No connection string configured. Set the DefaultConnection environment variable, add a ConnectionStrings:DefaultConnection entry to appsettings.json, or update the fallback in DesignTimeDbContextFactory.");
            }

            builder.UseSqlServer(conn);

            return new AppointmentBookingDbContext(builder.Options);
        }
    }
}
