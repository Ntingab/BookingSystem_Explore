using System;
using System.Security.Cryptography;
using System.Text;

namespace BookingSystem_Explore.Infrastructure.Security
{
    public class TokenService : ITokenService
    {
        public string GenerateToken()
        {
            // 32 bytes -> 256 bits, base64 for transport
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        public string HashToken(string token)
        {
            // Use SHA256 over the UTF8 bytes
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool Verify(string token, string hash)
        {
            var computed = HashToken(token);
            // constant time comparison
            return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(computed), Convert.FromBase64String(hash));
        }
    }
}
