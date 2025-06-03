using University_Schedule_Gateway.Models;

namespace University_Schedule_Gateway.Interfaces.Auth;

public interface IJwtProvider
{
    public string GenerateToken(User user);
}