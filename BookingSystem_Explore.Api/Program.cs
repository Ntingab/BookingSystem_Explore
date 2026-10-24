global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using System.Threading.RateLimiting;
using BookingSystem_Explore.BusinessLibrary;
using BookingSystem_Explore.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using BookingSystem_Explore.Infrastructure.Constants;
using Serilog;
using System.Security.Cryptography;

namespace BookingSystem_Explore.Api
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            ConfigureDevelopmentSecrets(builder);

            // Disable strict DI validation at build time to avoid runtime type resolution issues
            builder.Host.UseDefaultServiceProvider(options =>
            {
                options.ValidateOnBuild = false;
                options.ValidateScopes = false;
            });

            // Add services to the container.
            builder.Services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
            })
            .AddXmlSerializerFormatters();

            // Bind strongly-typed settings
            builder.Services.Configure<Options.ConnectionStringsOptions>(configuration.GetSection("ConnectionStrings"));
            builder.Services.Configure<Options.RateLimiterOptions>(configuration.GetSection("RateLimiter"));
            builder.Services.Configure<Options.FeatureFlagsOptions>(configuration.GetSection("FeatureFlags"));
            builder.Services.Configure<Options.CorsOptions>(configuration.GetSection("Cors"));
            builder.Services.Configure<Options.ClientAccessOptions>(configuration.GetSection("ClientAccess"));

            // Central service configuration (controllers, versioning, etc.)
            builder.Services.ConfigureServices(configuration);

            // Configure CORS policy from configuration (restrict origins in production)
            var corsOptions = configuration.GetSection("Cors").Get<Options.CorsOptions>() ?? new Options.CorsOptions();
            builder.Services.AddCors(opts =>
            {
                opts.AddPolicy("DefaultCors", policy =>
                {
                    if (corsOptions.AllowedOrigins != null && corsOptions.AllowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(corsOptions.AllowedOrigins).AllowAnyHeader().AllowAnyMethod();
                    }
                    else if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("IntegrationTests"))
                    {
                        policy.WithOrigins("https://localhost:5001", "https://localhost:7025")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                });
            });

            // OpenAPI (Swashbuckle) registration - use default configuration
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Basic health checks for liveness/readiness probes
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
                .AddCheck<BookingSystem_Explore.Api.HealthChecks.DbHealthCheck>("database");

            // Register infrastructure and business services (EF, repositories, token service, business services)
            // Allow skipping DB registration when running integration tests. The test host sets the
            // environment to 'IntegrationTests' which instructs the infrastructure to avoid adding
            // the SQL Server provider so tests can supply an in-memory provider.
            var skipDb = builder.Environment.IsEnvironment("IntegrationTests");
            builder.Services.AddInfrastructureServices(builder.Configuration, skipDb);
            builder.Services.AddBusinessLibraryServices();

            // If running outside development, allow client certificates on Kestrel
            if (!builder.Environment.IsDevelopment())
            {
                builder.Services.Configure<KestrelServerOptions>(options =>
                {
                    options.ConfigureHttpsDefaults(opt =>
                    {
                        opt.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                    });
                });
            }

            // Rate limiting (per-IP token bucket) to mitigate abuse and brute-force attacks.
            var configuredRateLimit = configuration.GetSection("RateLimiter").Get<Options.RateLimiterOptions>() ?? new Options.RateLimiterOptions();

            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var effectiveLimit = new Options.ClientRateLimitOptions
                    {
                        TokenLimit = configuredRateLimit.TokenLimit,
                        TokensPerPeriod = configuredRateLimit.TokensPerPeriod,
                        ReplenishmentMinutes = configuredRateLimit.ReplenishmentMinutes,
                        QueueLimit = configuredRateLimit.QueueLimit
                    };

                    var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "global";

                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey,
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = effectiveLimit.TokenLimit,
                            TokensPerPeriod = effectiveLimit.TokensPerPeriod,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(effectiveLimit.ReplenishmentMinutes),
                            AutoReplenishment = true,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = effectiveLimit.QueueLimit
                        }
                    );
                });

                // Standard HTTP status code returned when requests are rejected due to rate limiting
                options.RejectionStatusCode = 429;
            });

            // Initialize Serilog from configuration
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            var app = builder.Build();

            app.UseMiddleware<BookingSystem_Explore.Api.Middleware.ExceptionHandlingMiddleware>();

            // Apply CORS before client access checks so browser preflight gets CORS headers.
            app.UseCors("DefaultCors");

            app.UseRateLimiter();

            app.UseMiddleware<BookingSystem_Explore.Api.Middleware.ClientAccessMiddleware>();

            // Security headers: simple Content-Security-Policy; customize per requirements
            app.Use(async (context, next) =>
            {
                context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["Referrer-Policy"] = "no-referrer";
                await next();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("IntegrationTests"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/v{Versions.Version1}/swagger.json", "BookingSystem Explore API v1");
                    c.DocumentTitle = "BookingSystem Explore API";
                    c.RoutePrefix = string.Empty;
                });
            }

            // Expose health endpoints for probes
            app.MapHealthChecks("/health");         // general health
            app.MapHealthChecks("/health/ready");   // readiness probe
            app.MapHealthChecks("/health/live");    // liveness probe

            // Redirect HTTP requests to HTTPS
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            await app.RunAsync();
        }

        private static void ConfigureDevelopmentSecrets(WebApplicationBuilder builder)
        {
            var signingKey = builder.Configuration["ClientAccess:Jwt:SigningKey"];
            var phoneKey = builder.Configuration["Security:PhoneEncryption:Key"];
            var mayUseEphemeralSecrets = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("IntegrationTests");

            if (string.IsNullOrWhiteSpace(signingKey))
            {
                if (!mayUseEphemeralSecrets) throw new InvalidOperationException("ClientAccess:Jwt:SigningKey must be supplied by a secure configuration provider.");
                builder.Configuration["ClientAccess:Jwt:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            }

            if (string.IsNullOrWhiteSpace(phoneKey))
            {
                if (!mayUseEphemeralSecrets) throw new InvalidOperationException("Security:PhoneEncryption:Key must be supplied by a secure configuration provider.");
                builder.Configuration["Security:PhoneEncryption:Key"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            }
        }
    }
}
