using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BookingSystem_Explore.Ui.Services;

public class JwtTokenClient
{
    private readonly ApiClientSettings _settings;

    public JwtTokenClient(ApiClientSettings settings)
    {
        _settings = settings;
    }

    public string CreateToken(string scope)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.JwtIssuer,
            audience: _settings.JwtAudience,
            claims: [new Claim("scope", scope)],
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_settings.JwtExpiryMinutes),
            signingCredentials: credentials);

        return handler.WriteToken(token);
    }
}
