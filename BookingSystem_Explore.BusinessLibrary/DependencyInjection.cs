using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.BusinessLibrary.Notifications;
using BookingSystem_Explore.BusinessLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem_Explore.BusinessLibrary
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLibraryServices(this IServiceCollection services)
        {
            // Register business services (repositories and unit-of-work are provided by Infrastructure)
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<IAppointmentSlotService, AppointmentSlotService>();
            services.AddScoped<IAppointmentService, Services.AppointmentService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<INotificationService, NotificationService>();

            // Notification processor (uses repository abstractions)
            services.AddScoped<SimulatedNotificationProcessor>();

            return services;
        }
    }
}
