using BookingSystem_Explore.Infrastructure.Persistence;
using BookingSystem_Explore.Infrastructure.Repositories;
using BookingSystem_Explore.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem_Explore.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool skipDbRegistration = false)
        {
            var cfgUseInMemory = configuration["UseInMemoryDb"];
            var useInMemoryDb = string.Equals(cfgUseInMemory, "true", System.StringComparison.OrdinalIgnoreCase);
            if (!useInMemoryDb && string.IsNullOrWhiteSpace(cfgUseInMemory))
            {
                var useInMemoryEnv = System.Environment.GetEnvironmentVariable("USE_INMEMORY_DB");
                useInMemoryDb = string.Equals(useInMemoryEnv, "true", System.StringComparison.OrdinalIgnoreCase);
            }
            // If the test host or another registration already configured DbContextOptions for
            // AppointmentBookingDbContext (e.g. integration tests replacing it with InMemory),
            // avoid registering the SQL Server provider again to prevent multiple provider errors.
            if (!skipDbRegistration &&
                !useInMemoryDb &&
                !services.Any(sd => sd.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions<AppointmentBookingDbContext>)))
            {
                var conn = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(conn))
                {
                    throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be supplied by user secrets, an environment variable, or another secure configuration provider.");
                }

                services.AddDbContext<AppointmentBookingDbContext>(options => options.UseSqlServer(conn));
            }

            // Register EF repositories (implement Domain interfaces)
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.IBranchRepository, BranchRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.IServiceRepository, ServiceRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.IAppointmentSlotRepository, AppointmentSlotRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.ICustomerRepository, CustomerRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.INotificationRepository, NotificationRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.IUnitOfWork, EfUnitOfWork>();

            // Token service: implement Domain.Security.ITokenService
            services.AddSingleton<BookingSystem_Explore.Domain.Security.ITokenService, TokenService>();
            services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new PhoneEncryptionOptions
            {
                Key = configuration["Security:PhoneEncryption:Key"] ?? string.Empty
            }));
            services.AddSingleton<BookingSystem_Explore.Domain.Security.IPhoneEncryptionService, PhoneEncryptionService>();

            return services;
        }
    }
}
