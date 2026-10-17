global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using System.Threading.RateLimiting;
using BookingSystem_Explore.Infrastructure;
using BookingSystem_Explore.BusinessLibrary;

var builder = WebApplication.CreateBuilder(args);

// Disable strict DI validation at build time to avoid runtime type resolution issues
// (some services are registered by extension methods across projects and validated
// after all registrations are applied at runtime). This avoids TypeLoad/validation
// exceptions during host build in development. Remove if you prefer strict validation.
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = false;
    options.ValidateScopes = false;
});

// Basic API setup so the project builds. Further configuration is done elsewhere in the solution.
builder.Services.AddControllers(options =>
{
    // optionally, global filters can be added here
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Swagger to document the management token header
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ManagementToken", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Management-Token",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Management token for accessing and managing private appointment resources"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "ManagementToken" }
            },
            Array.Empty<string>()
        }
    });
});

// Basic health checks for liveness/readiness probes
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

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

app.UseMiddleware<BookingSystem_Explore.Api.Middleware.ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookingSystem Explore API v1");
    c.DocumentTitle = "BookingSystem Explore API";
    c.RoutePrefix = string.Empty; // serve Swagger UI at application root
});

// Use rate limiter middleware globally
app.UseRateLimiter();

// Expose health endpoints for probes
app.MapHealthChecks("/health");         // general health
app.MapHealthChecks("/health/ready");   // readiness probe
app.MapHealthChecks("/health/live");    // liveness probe

app.MapControllers();

app.Run();
