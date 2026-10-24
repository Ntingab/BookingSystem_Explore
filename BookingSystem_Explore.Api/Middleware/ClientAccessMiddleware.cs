using BookingSystem_Explore.Api.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookingSystem_Explore.Api.Middleware;

public class ClientAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClientAccessMiddleware> _logger;

    public ClientAccessMiddleware(RequestDelegate next, ILogger<ClientAccessMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptionsMonitor<ClientAccessOptions> options)
    {
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.Equals("/api/v1/auth/token", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!TryGetSessionId(context, out var sessionId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing required X-Session-Id header.");
            return;
        }

        context.TraceIdentifier = sessionId!;
        context.Response.Headers["X-Session-Id"] = sessionId;

        if (!TryGetClientId(context, out var clientId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing required client identifier.");
            return;
        }

        var configuredClients = options.CurrentValue.Clients;
        var client = configuredClients.FirstOrDefault(c => string.Equals(c.ClientId, clientId, StringComparison.Ordinal));
        if (client == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid client identifier.");
            return;
        }

        if (options.CurrentValue.RequireClientCertificate && !context.RequestServices.GetRequiredService<IHostEnvironment>().IsEnvironment("IntegrationTests"))
        {
            var certificate = context.Connection.ClientCertificate;
            var allowedThumbprints = client.AllowedCertificateThumbprints
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (allowedThumbprints.Count > 0)
            {
                if (certificate == null || string.IsNullOrWhiteSpace(certificate.Thumbprint) || !allowedThumbprints.Contains(certificate.Thumbprint))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Client certificate is missing or not allowed for this client.");
                    return;
                }
            }
        }

        var requiredScope = GetRequiredScope(context.Request);
        var clientAllowedScopes = client.Scopes.Select(s => s.ToLowerInvariant()).ToHashSet();
        var tokenScopes = ValidateAndGetJwtScopes(context, options.CurrentValue.Jwt, clientId!);

        if (tokenScopes.Count == 0)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing or invalid bearer token.");
            return;
        }

        if (!tokenScopes.Contains(requiredScope) || !clientAllowedScopes.Contains(requiredScope))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Client scope does not allow this operation.");
            return;
        }

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["SessionId"] = sessionId!,
                   ["ClientId"] = clientId!,
                   ["ClientScope"] = requiredScope
               }))
        {
            await _next(context);
        }
    }

    private static bool TryGetSessionId(HttpContext context, out string? sessionId)
    {
        sessionId = null;
        if (!context.Request.Headers.TryGetValue("X-Session-Id", out var sessionIdValues))
        {
            return false;
        }

        sessionId = sessionIdValues.FirstOrDefault();
        return !string.IsNullOrWhiteSpace(sessionId);
    }

    private static bool TryGetClientId(HttpContext context, out string? clientId)
    {
        clientId = null;

        if (!context.Request.Headers.TryGetValue("X-Client-Id", out var idValues))
        {
            return false;
        }

        clientId = idValues.FirstOrDefault();
        return !string.IsNullOrWhiteSpace(clientId);
    }

    private static HashSet<string> ValidateAndGetJwtScopes(HttpContext context, JwtClientOptions jwtOptions, string clientId)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authValues))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var auth = authValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var token = auth["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out _);

            if (!string.Equals(principal.FindFirstValue("client_id"), clientId, StringComparison.Ordinal))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return ExtractScopes(principal);
        }
        catch
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static HashSet<string> ExtractScopes(ClaimsPrincipal principal)
    {
        var scopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var scopeClaims = principal.Claims.Where(c => c.Type == "scope" || c.Type == "scp").Select(c => c.Value);

        foreach (var claimValue in scopeClaims)
        {
            foreach (var scope in claimValue.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                scopes.Add(scope.ToLowerInvariant());
            }
        }

        return scopes;
    }

    private static string GetRequiredScope(HttpRequest request)
    {
        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method))
        {
            return "read";
        }

        return HttpMethods.IsPost(request.Method) && request.Path.Equals("/api/v1/appointments", StringComparison.OrdinalIgnoreCase)
            ? "booking:create"
            : HttpMethods.IsPatch(request.Method) && request.Path.StartsWithSegments("/api/v1/appointments", StringComparison.OrdinalIgnoreCase)
                ? "booking:manage"
                : "admin";
    }
}
