using BookingSystem_Explore.Api.Extensions;

namespace BookingSystem_Explore.Api
{
    public static class ServiceConfigurator
    {
        /// <summary>
        /// Configures services and dependencies for the application.
        /// </summary>
        /// <param name="services">The service collection to which services are added.</param>
        /// <param name="configuration">The application configuration to retrieve connection strings and secrets.</param>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add services to the container.

            services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
            });

            // Keep Swagger/OpenAPI registration in Program.cs to avoid duplicate registrations.
            services.AddVersionServices();

            // CORS policy registration is configured in Program.cs to allow configuration-driven policies.

            return services;
        }
    }
}
