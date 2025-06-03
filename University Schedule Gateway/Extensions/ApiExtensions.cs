using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace University_Schedule_Gateway.Extensions;

public static class ApiExtensions
{
    public static void AddPostgres(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        // Оставляем AddDbContext, если ApplicationContext используется где-то еще
        // напрямую в запросах (как Scoped). Если нет - можно убрать.
        services.AddDbContext<AuthenticationContext>(options =>
            options.UseNpgsql(connectionString));

        // *** Добавляем регистрацию ФАБРИКИ DbContext ***
        // Это позволит создавать контекст с нужным lifetime вручную.
        services.AddDbContextFactory<AuthenticationContext>(options =>
                options.UseNpgsql(connectionString),
            ServiceLifetime.Scoped); // Фабрика может быть Scoped или Singleton,
        // Scoped чаще всего подходит.
    }

    public static void AddApiAuthentication(this IServiceCollection services, IConfigurationSection jwtSection)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var secretKey = jwtSection.GetValue<string>("SecretKey");
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["cookies"];
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AuthenticatedUserPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });
    }
}