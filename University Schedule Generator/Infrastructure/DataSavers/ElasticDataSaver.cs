using Elastic.Clients.Elasticsearch;
using University_Schedule_Generator.Infrastructure.Generators.Data;
using University_Schedule_Generator.Interfaces.DataSaver;

namespace University_Schedule_Generator.Services.DataSavers;

public class ElasticDataSaver : IDataSaver<GeneratedData>
{
    private readonly ElasticsearchClient _esClient;
    private readonly ILogger<ElasticDataSaver> _logger;

    private class MaterialElasticDoc
    {
        /* ... как раньше ... */
        public int id { get; set; }
        public int id_lect { get; set; }
        public string name { get; set; }
        public string lecture_text { get; set; }
    }

    public ElasticDataSaver(ElasticsearchClient esClient, ILogger<ElasticDataSaver> logger)
    {
        _esClient = esClient;
        _logger = logger;
    }

    public async Task<SaveResult> SaveAsync(GeneratedData data)
    {
        _logger.LogInformation("Saving materials to Elasticsearch...");
        if (data.MaterialElastics.Any())
        {
            var elasticDocs = data.MaterialElastics.Select(m => new MaterialElasticDoc
            {
                id = m.Id,
                id_lect = m.LectureId,
                name = m.Name,
                lecture_text = m.Content
            });

            var bulkResponse = await _esClient.BulkAsync(b => b
                .Index("materials") // Имя индекса
                .IndexMany(elasticDocs)
            );

            if (!bulkResponse.IsValidResponse)
            {
                _logger.LogError("Error saving to Elasticsearch: {ErrorReason}", bulkResponse.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Elasticsearch save complete.");
            }
        }
        else
        {
            _logger.LogWarning("No MaterialElastic objects to save to Elasticsearch.");
        }
        return new SaveResult(true, "Elasticsearch save complete.");
    }
}