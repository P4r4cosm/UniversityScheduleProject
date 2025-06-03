using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

namespace University_Schedule_Generator.Endpoints;

public static class MongoEndpoints
{
    public static RouteGroupBuilder MapMongoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(""); // Опциональный под-префикс
        group.MapGet("/mongo_test", async (IMongoDatabase db) =>
        {
            try
            {
                var collection = db.GetCollection<BsonDocument>("universities");
                var documents = await collection.Find(new BsonDocument()).ToListAsync();

                // Преобразование BSON в JSON-строку
                var jsonResult = documents.ToJson(new JsonWriterSettings { Indent = true });

                return Results.Text(jsonResult, "application/json");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Ошибка: {ex.Message}");
            }
        }).WithName("GetMongoData").WithTags("Mongo");
        return group;
    }
}