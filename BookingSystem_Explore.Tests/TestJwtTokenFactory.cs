using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BookingSystem_Explore.Tests;

internal static class TestJwtTokenFactory
{
    public static string Create(string scope)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Change-This-Jwt-Signing-Key-AtLeast-32-Bytes-Long-123456"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "BookingSystem_Explore.Ui",
            audience: "BookingSystem_Explore.Api",
            claims: [new Claim("scope", scope)],
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
