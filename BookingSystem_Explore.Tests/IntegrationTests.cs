using System.Net.Http.Json;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BookingSystem_Explore.Infrastructure.Persistence;
using BookingSystem_Explore.Infrastructure.Security;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Xunit;

namespace BookingSystem_Explore.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<BookingSystem_Explore.Api.Program>>
    {
        private readonly WebApplicationFactory<BookingSystem_Explore.Api.Program> _factory;

        public IntegrationTests(WebApplicationFactory<BookingSystem_Explore.Api.Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                // Run the app under a distinct environment so Program.Main can detect it
                // and skip SQL registration early in startup. Configure host configuration
                // so the environment is set before the app builds.
                // Set the hosting environment via IWebHostBuilder settings so Program detects it early
                builder.UseSetting("environment", "IntegrationTests");
                // Tell the application to use the in-memory database provider when building the host
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    // Use MemoryConfigurationSource to avoid requiring the AddInMemoryCollection extension
                    var data = new Dictionary<string, string?> { ["UseInMemoryDb"] = "true" }
                        .Where(kv => kv.Value != null)
                        .ToDictionary(kv => kv.Key, kv => kv.Value!);
                    cfg.Add(new Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource { InitialData = data });
                });
                builder.ConfigureServices(services =>
                {
                    // replace real DbContext with in-memory for tests
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppointmentBookingDbContext>));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddDbContext<AppointmentBookingDbContext>(options => options.UseInMemoryDatabase("integration_db"));

                    // Seed deterministic data for integration tests
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppointmentBookingDbContext>();
                        // ensure database is created
                        db.Database.EnsureCreated();

                        var store = new BookingSystem_Explore.BusinessLibrary.Repositories.InMemoryDataStore();

                        // clear existing
                        db.Branches.RemoveRange(db.Branches);
                        db.Services.RemoveRange(db.Services);
                        db.BranchServices.RemoveRange(db.BranchServices);
                        db.AppointmentSlots.RemoveRange(db.AppointmentSlots);
                        db.Notifications.RemoveRange(db.Notifications);
                        db.Appointments.RemoveRange(db.Appointments);
                        db.Customers.RemoveRange(db.Customers);
                        db.SaveChanges();

                        db.Branches.AddRange(store.Branches);
                        db.Services.AddRange(store.Services);
                        db.BranchServices.AddRange(store.BranchServices);
                        db.AppointmentSlots.AddRange(store.Slots);
                        db.SaveChanges();
                    }
                });
            });
        }

        [Fact]
        public async Task BookingJourney_EndToEnd_Works()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add("X-Client-Id", "booking-ui");
            client.DefaultRequestHeaders.Add("X-Client-Secret", "Change-This-Client-Secret");
            client.DefaultRequestHeaders.Add("X-Session-Id", Guid.NewGuid().ToString("N"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwtTokenFactory.Create("read write"));

            var phoneEncryptionService = new PhoneEncryptionService(
                Options.Create(new PhoneEncryptionOptions { Key = "Change-This-Phone-Encryption-Key" }));

            // 1 - get branches
            var branchesResp = await client.GetAsync("/api/v1/branches");
            branchesResp.EnsureSuccessStatusCode();
            var branches = await branchesResp.Content.ReadFromJsonAsync<IReadOnlyList<BookingSystem_Explore.BusinessLibrary.DTOs.Responses.BranchResponse>>();
            Assert.NotNull(branches);
            var branchId = branches!.First().BranchId;

            // 2 - get branch services
            var svcResp = await client.GetAsync($"/api/v1/branches/{branchId}/services");
            svcResp.EnsureSuccessStatusCode();
            var svcs = await svcResp.Content.ReadFromJsonAsync<IReadOnlyList<BranchServiceResponse>>();
            Assert.NotNull(svcs);
            var branchServiceId = svcs!.First().BranchServiceId;

            // 3 - get available slots for today
            var date = DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-dd");
            var slotsResp = await client.GetAsync($"/api/v1/appointment-slots?branchServiceId={branchServiceId}&date={date}");
            slotsResp.EnsureSuccessStatusCode();
            var slots = await slotsResp.Content.ReadFromJsonAsync<IReadOnlyList<BookingSystem_Explore.BusinessLibrary.DTOs.Responses.AppointmentSlotResponse>>();
            Assert.NotNull(slots);
            var slotId = slots!.First().AppointmentSlotId;

            // 4 - book appointment
            var request = new CreateAppointmentRequest
            {
                BranchServiceId = branchServiceId,
                AppointmentSlotId = slotId,
                Customer = new CreateAppointmentRequest.CustomerDto { FirstName = "IT", LastName = "Test", EmailAddress = "it@test.com", PhoneNumber = phoneEncryptionService.Encrypt("123") },
                Notes = "integration test"
            };

            var createResp = await client.PostAsJsonAsync("/api/v1/appointments", request);
            createResp.EnsureSuccessStatusCode();
            var created = await createResp.Content.ReadFromJsonAsync<AppointmentCreatedResponse>();
            Assert.NotNull(created);
            Assert.False(string.IsNullOrEmpty(created!.ManagementToken));

            // 5 - get appointment with header token
            client.DefaultRequestHeaders.Add("X-Management-Token", created.ManagementToken);
            var getResp = await client.GetAsync($"/api/v1/appointments/{created.AppointmentId}");
            getResp.EnsureSuccessStatusCode();

            // 6 - cancel
            var cancelResp = await client.PatchAsync($"/api/v1/appointments/{created.AppointmentId}/cancel", null);
            cancelResp.EnsureSuccessStatusCode();
        }
    }
}
