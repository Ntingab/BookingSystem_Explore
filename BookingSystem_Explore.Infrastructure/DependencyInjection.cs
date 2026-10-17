using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BookingSystem_Explore.Infrastructure.Persistence;
using BookingSystem_Explore.Domain.Interfaces.Repositories;
using BookingSystem_Explore.Infrastructure.Repositories;
using BookingSystem_Explore.Infrastructure.Security;

namespace BookingSystem_Explore.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppointmentBookingDbContext>(options => options.UseSqlServer(conn));

            // Register EF repositories (implement Domain interfaces)
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.IBranchRepository, BranchRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.IServiceRepository, ServiceRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.IAppointmentSlotRepository, AppointmentSlotRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.ICustomerRepository, CustomerRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.Repositories.INotificationRepository, NotificationRepository>();
            services.AddScoped<BookingSystem_Explore.Domain.Interfaces.IUnitOfWork, EfUnitOfWork>();

            // Token service: implement Domain.Security.ITokenService
            // Infrastructure TokenService implements Domain.Security.ITokenService. The Infrastructure ITokenService
            // duplicate caused type mismatch; register implementation via factory to avoid compile-time generic constraint.
            services.AddSingleton<BookingSystem_Explore.Domain.Security.ITokenService>(_ => new TokenService());

            return services;
        }
    }
}
