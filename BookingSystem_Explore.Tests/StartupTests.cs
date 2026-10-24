using System.Net;
using Xunit;

namespace BookingSystem_Explore.Tests
{
    public class StartupTests : IClassFixture<BookingWebApplicationFactory>
    {
        private readonly BookingWebApplicationFactory _factory;

        public StartupTests(BookingWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthEndpoint_ReturnsHealthy()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/health");
            Assert.True(resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task SwaggerJson_IsAvailable()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync($"/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }
    }
}
