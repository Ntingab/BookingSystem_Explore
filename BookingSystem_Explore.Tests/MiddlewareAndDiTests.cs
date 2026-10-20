using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using BookingSystem_Explore.Infrastructure.Persistence;
using Xunit;

namespace BookingSystem_Explore.Tests
{
    public class MiddlewareAndDiTests : IClassFixture<WebApplicationFactory<BookingSystem_Explore.Api.Program>>
    {
        private readonly WebApplicationFactory<BookingSystem_Explore.Api.Program> _factory;

        public MiddlewareAndDiTests(WebApplicationFactory<BookingSystem_Explore.Api.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ExceptionHandlingMiddleware_HandlesExceptions_ReturnsProblemJson()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            // Test endpoint is versioned under the API route template
            var resp = await client.GetAsync("/api/v1/test/throw");
            Assert.Equal(HttpStatusCode.InternalServerError, resp.StatusCode);
            Assert.Equal("application/problem+json", resp.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public void Services_Are_Registered_In_DI()
        {
            var services = _factory.Services;
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            // Check DB context can be resolved
            var db = sp.GetService<AppointmentBookingDbContext>();
            Assert.NotNull(db);

            // Check configuration options are registered
            var connOpt = sp.GetService<Microsoft.Extensions.Options.IOptions<BookingSystem_Explore.Api.Options.ConnectionStringsOptions>>();
            Assert.NotNull(connOpt);
        }
    }
}
