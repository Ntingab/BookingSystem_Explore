namespace BookingSystem_Explore.Domain.Security
{
    public interface ITokenService
    {
        string GenerateToken();
        string HashToken(string token);
        bool Verify(string token, string hash);
    }
}
