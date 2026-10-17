using System;
namespace BookingSystem_Explore.Infrastructure.Security
{
    public interface ITokenService
    {
        string GenerateToken();
        string HashToken(string token);
        bool Verify(string token, string hash);
    }
}
