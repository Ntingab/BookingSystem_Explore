using System.Security.Cryptography;
using System.Text;

namespace BookingSystem_Explore.Infrastructure.Security
{
    public class TokenService : BookingSystem_Explore.Domain.Security.ITokenService
    {
        public string GenerateToken()
        {
            // 32 bytes -> 256 bits, use Base64Url (URL-safe, no padding) for transport so token is safe in headers/urls
            var bytes = RandomNumberGenerator.GetBytes(32);
            var b64 = Convert.ToBase64String(bytes);
            // base64url: replace +/, remove padding
            var b64Url = b64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return b64Url;
        }

        public string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        public bool Verify(string token, string hash)
        {
            if (string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(hash))
            {
                return false;
            }

            try
            {
                var bytes1 = Convert.FromBase64String(token.Trim());
                var bytes2 = Convert.FromBase64String(hash.Trim());

                return CryptographicOperations.FixedTimeEquals(
                    bytes1,
                    bytes2);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
