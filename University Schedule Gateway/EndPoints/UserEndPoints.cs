using University_Schedule_Gateway.Contracts;
using University_Schedule_Gateway.Services;

namespace University_Schedule_Gateway.EndPoints;

public static class UserEndPoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("register", Register);
        app.MapPost("login", Login);
        return app;
    }

    public static async Task<IResult> Register(RegisterUserRequest request, UserService userServices)
    {
        await userServices.Register(request.Name, request.Password);
        return Results.Ok();
    }
    public static async Task<IResult> Login(LoginUserRequest request,UserService userServices, HttpContext httpContext)
    {
        var token = await userServices.Login(request.Name, request.Password);
        if (token == null)
        {
            return Results.Unauthorized(); // Возвращаем 401 Unauthorized, если вход не удался
        }
        
        httpContext.Response.Cookies.Append("cookies", token);
        
        return Results.Ok(token); // Возвращаем токен в теле ответа
    }
}