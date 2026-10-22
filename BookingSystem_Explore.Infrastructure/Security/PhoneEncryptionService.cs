using System.Security.Cryptography;
using System.Text;
using BookingSystem_Explore.Domain.Security;
using Microsoft.Extensions.Options;

namespace BookingSystem_Explore.Infrastructure.Security
{
    public class PhoneEncryptionService : IPhoneEncryptionService
    {
        private const string Prefix = "enc:";
        private readonly byte[] _key;

        public PhoneEncryptionService(IOptions<PhoneEncryptionOptions> options)
        {
            var configuredKey = options.Value.Key;
            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                throw new InvalidOperationException("Phone encryption key is not configured.");
            }

            _key = SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                return string.Empty;
            }

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var payload = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);

            return Prefix + Convert.ToBase64String(payload);
        }

        public bool TryDecrypt(string cipherText, out string plainText)
        {
            plainText = string.Empty;

            if (string.IsNullOrWhiteSpace(cipherText) || !cipherText.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                var payload = Convert.FromBase64String(cipherText[Prefix.Length..]);
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var ivLength = aes.BlockSize / 8;
                if (payload.Length <= ivLength)
                {
                    return false;
                }

                var iv = new byte[ivLength];
                var cipherBytes = new byte[payload.Length - ivLength];
                Buffer.BlockCopy(payload, 0, iv, 0, ivLength);
                Buffer.BlockCopy(payload, ivLength, cipherBytes, 0, cipherBytes.Length);

                using var decryptor = aes.CreateDecryptor(aes.Key, iv);
                var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                plainText = Encoding.UTF8.GetString(plainBytes);
                return !string.IsNullOrWhiteSpace(plainText);
            }
            catch
            {
                return false;
            }
        }
    }
}
