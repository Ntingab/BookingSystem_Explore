global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using System.Threading.RateLimiting;
using BookingSystem_Explore.Infrastructure;
using BookingSystem_Explore.BusinessLibrary;

var builder = WebApplication.CreateBuilder(args);

// Basic API setup so the project builds. Further configuration is done elsewhere in the solution.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register infrastructure and business services (EF, repositories, token service, business services)
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddBusinessLibraryServices();

// Rate limiting (per-IP token bucket) to mitigate abuse and brute-force attacks.
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 20,
                TokensPerPeriod = 20,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }
        )
    );

    // Standard HTTP status code returned when requests are rejected due to rate limiting
    options.RejectionStatusCode = 429;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookingSystem Explore API v1");
    c.DocumentTitle = "BookingSystem Explore API";
    c.RoutePrefix = string.Empty; // serve Swagger UI at application root
});

// Use rate limiter middleware globally
app.UseRateLimiter();

app.MapControllers();

app.Run();
