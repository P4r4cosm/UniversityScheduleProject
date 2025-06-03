using Elastic.Clients.Elasticsearch;

namespace University_Schedule_Generator.Endpoints;

public static class ElasticSearchEndpoints
{
    public static RouteGroupBuilder MapElasticSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(""); // Опциональный под-префикс
        app.MapGet("/elastic_test", async (ElasticsearchClient esClient) =>
        {
            var response = await esClient.SearchAsync<dynamic>(s => s
                .Index("materials")
                .Query(q => q
                    .Match(m => m
                        .Field("lecture_text") // Поле в виде строки
                        .Query("Использование")
                    )
                )
            );

            // Возвращаем сырые JSON-данные
            return Results.Ok(response.Documents);
        }) .WithName("GetElasticData")
        .WithTags("Elastic");
        return group;
    }
}