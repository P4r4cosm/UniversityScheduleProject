namespace University_Schedule_Gateway.Interfaces.Auth;

public interface IPasswordHasher
{
    string GenerateHash(string password);
    
    bool VerifyHash(string password, string hash);
}