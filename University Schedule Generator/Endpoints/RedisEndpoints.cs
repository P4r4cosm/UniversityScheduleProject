using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc;
namespace University_Schedule_Generator.Endpoints;

public static class RedisEndpoints
{
    public static RouteGroupBuilder MapRedisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(""); // Опциональный под-префикс
        group.MapGet("/redis_test", async ([FromServices] IConnectionMultiplexer mux) =>
        {
            var redis = mux.GetDatabase();
            var hashEntries = await redis.HashGetAllAsync("student:1");
            var result = hashEntries.ToDictionary(
                entry => entry.Name.ToString(),
                entry => entry.Value.ToString()
            );
            return result;
        })
        .WithName("GetRedisData")
        .WithTags("Redis");
        return group;
    }
}