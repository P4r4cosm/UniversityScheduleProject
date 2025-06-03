using Neo4j.Driver;

namespace University_Schedule_Generator.Endpoints;

public static class Neo4jEndpoints
{
    public static RouteGroupBuilder MapNeo4jEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(""); // Опциональный под-префикс
        app.MapGet("/neo4j_test", async (IDriver driver) =>
            {
                await using var session = driver.AsyncSession();
                try 
                {
                    var result = await session.RunAsync("MATCH (n) RETURN n LIMIT 25");
                    var records = await result.ToListAsync();
                    var nodes = records.Select(record => 
                        record["n"].As<INode>().Properties.ToDictionary(
                            p => p.Key, 
                            p => p.Value?.ToString() ?? "null"
                        )
                    ).ToList();
                    return Results.Json(nodes);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Ошибка: {ex.Message}");
                }
            })
            .WithName("GetNeo4jNodes")
            .WithTags("Neo4j");
        return group;
    }
}