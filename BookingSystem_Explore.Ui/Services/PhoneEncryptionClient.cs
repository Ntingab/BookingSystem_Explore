using System.Security.Cryptography;
using System.Text;

namespace BookingSystem_Explore.Ui.Services;

public class PhoneEncryptionClient
{
    private const string Prefix = "enc:";
    private readonly byte[] _key;

    public PhoneEncryptionClient(string configuredKey)
    {
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            throw new InvalidOperationException("Phone encryption key is missing.");
        }

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
    }

    public string Encrypt(string plainText)
    {
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
}
