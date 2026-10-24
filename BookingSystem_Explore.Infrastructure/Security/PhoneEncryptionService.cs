using System.Security.Cryptography;
using System.Text;
using BookingSystem_Explore.Domain.Security;
using Microsoft.Extensions.Options;

namespace BookingSystem_Explore.Infrastructure.Security
{
    public class PhoneEncryptionService : IPhoneEncryptionService
    {
        private const string Prefix = "enc:v2:";
        private const string LegacyPrefix = "enc:";
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

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var nonce = RandomNumberGenerator.GetBytes(12);
            var tag = new byte[16];
            var cipherBytes = new byte[plainBytes.Length];
            using var aes = new AesGcm(_key, tag.Length);
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

            var payload = new byte[nonce.Length + tag.Length + cipherBytes.Length];
            Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
            Buffer.BlockCopy(cipherBytes, 0, payload, nonce.Length + tag.Length, cipherBytes.Length);

            return Prefix + Convert.ToBase64String(payload);
        }

        public bool TryDecrypt(string cipherText, out string plainText)
        {
            plainText = string.Empty;

            if (string.IsNullOrWhiteSpace(cipherText))
            {
                return false;
            }

            try
            {
                if (cipherText.StartsWith(Prefix, StringComparison.Ordinal))
                {
                    var payload = Convert.FromBase64String(cipherText[Prefix.Length..]);
                    if (payload.Length <= 28) return false;

                    var nonce = payload.AsSpan(0, 12);
                    var tag = payload.AsSpan(12, 16);
                    var authenticatedCipherBytes = payload.AsSpan(28);
                    var authenticatedPlainBytes = new byte[authenticatedCipherBytes.Length];
                    using var authenticatedAes = new AesGcm(_key, tag.Length);
                    authenticatedAes.Decrypt(nonce, authenticatedCipherBytes, tag, authenticatedPlainBytes);
                    plainText = Encoding.UTF8.GetString(authenticatedPlainBytes);
                    return !string.IsNullOrWhiteSpace(plainText);
                }

                if (!cipherText.StartsWith(LegacyPrefix, StringComparison.Ordinal)) return false;
                var legacyPayload = Convert.FromBase64String(cipherText[LegacyPrefix.Length..]);
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var ivLength = aes.BlockSize / 8;
                if (legacyPayload.Length <= ivLength)
                {
                    return false;
                }

                var iv = new byte[ivLength];
                var cipherBytes = new byte[legacyPayload.Length - ivLength];
                Buffer.BlockCopy(legacyPayload, 0, iv, 0, ivLength);
                Buffer.BlockCopy(legacyPayload, ivLength, cipherBytes, 0, cipherBytes.Length);

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
