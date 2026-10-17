using System.Net.Http.Json;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BookingSystem_Explore.Infrastructure.Persistence;
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
                builder.ConfigureServices(services =>
                {
                    // replace real DbContext with in-memory for tests
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppointmentBookingDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppointmentBookingDbContext>(options => options.UseInMemoryDatabase("integration_db"));
                });
            });
        }

        [Fact]
        public async Task BookingJourney_EndToEnd_Works()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            // 1 - get branches
            var branchesResp = await client.GetAsync("/api/branches");
            branchesResp.EnsureSuccessStatusCode();
            var branches = await branchesResp.Content.ReadFromJsonAsync<IReadOnlyList<BookingSystem_Explore.BusinessLibrary.DTOs.Responses.BranchResponse>>();
            Assert.NotNull(branches);
            var branchId = branches!.First().BranchId;

            // 2 - get branch services
            var svcResp = await client.GetAsync($"/api/branches/{branchId}/services");
            svcResp.EnsureSuccessStatusCode();
            var svcs = await svcResp.Content.ReadFromJsonAsync<IReadOnlyList<ServiceResponse>>();
            Assert.NotNull(svcs);
            var branchServiceId = svcs!.First().ServiceId; // note: BranchServiceId not exposed, assume service id

            // 3 - get available slots for today
            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var slotsResp = await client.GetAsync($"/api/appointment-slots?branchServiceId={branchServiceId}&date={date}");
            slotsResp.EnsureSuccessStatusCode();
            var slots = await slotsResp.Content.ReadFromJsonAsync<IReadOnlyList<BookingSystem_Explore.BusinessLibrary.DTOs.Responses.AppointmentSlotResponse>>();
            Assert.NotNull(slots);
            var slotId = slots!.First().AppointmentSlotId;

            // 4 - book appointment
            var request = new CreateAppointmentRequest
            {
                BranchServiceId = branchServiceId,
                AppointmentSlotId = slotId,
                Customer = new CreateAppointmentRequest.CustomerDto { FirstName = "IT", LastName = "Test", EmailAddress = "it@test.com", PhoneNumber = "123" },
                Notes = "integration test"
            };

            var createResp = await client.PostAsJsonAsync("/api/appointments", request);
            createResp.EnsureSuccessStatusCode();
            var created = await createResp.Content.ReadFromJsonAsync<AppointmentCreatedResponse>();
            Assert.NotNull(created);
            Assert.False(string.IsNullOrEmpty(created!.ManagementToken));

            // 5 - get appointment with header token
            client.DefaultRequestHeaders.Add("X-Management-Token", created.ManagementToken);
            var getResp = await client.GetAsync($"/api/appointments/{created.AppointmentId}");
            getResp.EnsureSuccessStatusCode();

            // 6 - cancel
            var cancelResp = await client.PatchAsync($"/api/appointments/{created.AppointmentId}/cancel", null);
            cancelResp.EnsureSuccessStatusCode();
        }
    }
}
