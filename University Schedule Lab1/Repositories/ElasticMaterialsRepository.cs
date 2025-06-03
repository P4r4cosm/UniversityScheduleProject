using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using University_Schedule_Lab1;

public class ElasticMaterialsRepository
{
    private readonly ElasticsearchClient _elasticClient;

    public ElasticMaterialsRepository(ElasticsearchClient elasticClient) =>
        _elasticClient = elasticClient;

    public class MaterialElasticDoc
    {
        /* ... как раньше ... */
        public int id { get; set; }
        public int id_lect { get; set; }
        public string name { get; set; }
        public string lecture_text { get; set; }
    }

    public async Task<List<int>> GetMaterialElasticByTextAsync(string text, int limit)
    {
        var response = await _elasticClient.SearchAsync<MaterialElasticDoc>(s => s
            .Index("materials")
            .Query(q => q
                .Match(m => m
                    .Field(f => f.lecture_text) // Тип MaterialElasticDoc явно не требуется
                    .Query(text)
                )
            )
            .Size(limit));
        // Извлекаем значения id_lect из документов
        return response.Documents
            .Select(d => d.id_lect)
            .ToList();
    }
}