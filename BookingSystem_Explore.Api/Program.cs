global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using System.Threading.RateLimiting;
using BookingSystem_Explore.BusinessLibrary;
using BookingSystem_Explore.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using BookingSystem_Explore.Infrastructure.Constants;
using Serilog;

namespace BookingSystem_Explore.Api
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

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
                    else
                    {
                        // development fallback
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                });
            });

            // OpenAPI (Swashbuckle) registration - centralized here to avoid mixing providers
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc($"v{BookingSystem_Explore.Infrastructure.Constants.Versions.Version1}", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "BookingSystem Explore API",
                    Version = $"v{BookingSystem_Explore.Infrastructure.Constants.Versions.Version1}"
                });

                // Include XML comments if present
                var xmlFile = System.IO.Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().Location, ".xml");
                if (System.IO.File.Exists(xmlFile))
                {
                    options.IncludeXmlComments(xmlFile);
                }

                // Management token header security definition
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
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
                .AddCheck<BookingSystem_Explore.Api.HealthChecks.DbHealthCheck>("database");

            // Register infrastructure and business services (EF, repositories, token service, business services)
            builder.Services.AddInfrastructureServices(builder.Configuration);
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

            // Initialize Serilog from configuration
            Serilog.Log.Logger = new Serilog.LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            var app = builder.Build();

            app.UseMiddleware<BookingSystem_Explore.Api.Middleware.ExceptionHandlingMiddleware>();

            // Security headers: simple Content-Security-Policy; customize per requirements
            app.Use(async (context, next) =>
            {
                context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";
                await next();
            });

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v{Versions.Version1}/swagger.json", "BookingSystem Explore API v1");
                c.DocumentTitle = "BookingSystem Explore API";
                c.RoutePrefix = string.Empty; // serve Swagger UI at application root
            });

            // Use CORS policy
            app.UseCors("DefaultCors");

            // Use rate limiter middleware globally
            app.UseRateLimiter();

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
    }
}
