using BookingSystem_Explore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem_Explore.Tests;

public sealed class BookingWebApplicationFactory : WebApplicationFactory<BookingSystem_Explore.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureServices(services =>
        {
            services.AddDbContext<AppointmentBookingDbContext>(options =>
                options.UseInMemoryDatabase($"BookingSystemTests-{Guid.NewGuid():N}"));
        });
    }
}
