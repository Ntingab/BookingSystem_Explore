using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookingSystem_Explore.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookingSystem_Explore.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IOptionsMonitor<ClientAccessOptions> _options;

    public AuthController(IOptionsMonitor<ClientAccessOptions> options)
    {
        _options = options;
    }

    [HttpPost("token")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<TokenResponse> CreateToken([FromBody] TokenRequest request)
    {
        var options = _options.CurrentValue;
        var client = options.Clients.FirstOrDefault(candidate =>
            string.Equals(candidate.ClientId, request.ClientId, StringComparison.Ordinal));

        if (client is null)
        {
            return BadRequest();
        }

        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(Math.Clamp(options.Jwt.ExpiryMinutes, 1, 60));
        var claims = new List<Claim>
        {
            new("client_id", client.ClientId),
            new("scope", string.Join(' ', client.Scopes))
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Jwt.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: options.Jwt.Issuer,
            audience: options.Jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return Ok(new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc));
    }

    public sealed class TokenRequest
    {
        public string ClientId { get; set; } = string.Empty;
    }

    public sealed class TokenResponse
    {
        public TokenResponse()
        {
        }

        public TokenResponse(string accessToken, DateTime expiresAtUtc)
        {
            AccessToken = accessToken;
            ExpiresAtUtc = expiresAtUtc;
        }

        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
