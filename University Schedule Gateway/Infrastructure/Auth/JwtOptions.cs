namespace University_Schedule_Gateway.Infrastructure.Auth;

public class JwtOptions
{
    public string SecretKey { get; set; }
    public int ExpireHours { get; set; }
}