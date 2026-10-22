namespace BookingSystem_Explore.Domain.Security
{
    public interface IPhoneEncryptionService
    {
        string Encrypt(string plainText);
        bool TryDecrypt(string cipherText, out string plainText);
    }
}
