using System.Xml.Serialization;
using University_Schedule_Gateway.EndPoints;
using University_Schedule_Gateway.Extensions;
using University_Schedule_Gateway.Infrastructure.Auth;
using University_Schedule_Gateway.Interfaces.Auth;
using University_Schedule_Gateway.Repositories;
using University_Schedule_Gateway.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
builder.Logging.AddConsole();
// Swagger
services.AddEndpointsApiExplorer(); // Нужно для Minimal APIs
services.AddSwaggerGen(options =>
{
    // Документ для API самого шлюза (например, UserEndpoints)
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "University Schedule Gateway API",
        Version = "v1"
    });
    options.SwaggerDoc("generatorservice_v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Generator Service API",
        Version = "v1"
    });
});

services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
//добавляем базы
services.AddPostgres(builder.Configuration);

//Сервисы для JWT
services.AddScoped<UserRepository>();
services.AddScoped<IJwtProvider, JwtProvider>();
services.AddScoped<UserService>();
services.AddScoped<IPasswordHasher, PasswordHasher>();

// Ваша существующая настройка аутентификации
services.AddApiAuthentication(
    builder.Configuration.GetSection("JwtOptions"));

// 4. Добавление сервисов YARP
services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy")); // Загружаем конфигурацию из appsettings.json

var app = builder.Build();

// Размещаем UseHttpsRedirection РАНО
app.UseHttpsRedirection(); // <-- Перемещено сюда



// Явно добавляем маршрутизацию (хорошая практика)
app.UseRouting(); // <-- Добавлено для ясности

// Аутентификация и Авторизация ПОСЛЕ HttpsRedirection и UseRouting
app.UseAuthentication();
app.UseAuthorization();

// Swagger - обычно размещается после HttpsRedirection, но до Auth/Routing
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Генерирует swagger.json для ШЛЮЗА
    app.UseSwaggerUI(options =>
    {
        // Конечная точка для Swagger самого шлюза
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API V1");
        options.SwaggerEndpoint("https://localhost:7157/swagger/v1/swagger.json", "Generator Service V1"); 
        // Размещаем UI в корне (если хотите)
        options.RoutePrefix = string.Empty; // "" для доступа по /
    });
}
//Это middleware является терминальным для совпавших маршрутов.
app.UseEndpoints(endpoints =>
{
    endpoints?.MapUserEndpoints(); 
});
app.MapReverseProxy();

app.Run();