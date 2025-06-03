using MongoDB.Driver;
using University_Schedule_Generator.Infrastructure.Generators.Data;
using University_Schedule_Generator.Interfaces.DataSaver;

namespace University_Schedule_Generator.Services.DataSavers;

public class MongoDataSaver : IDataSaver<GeneratedData>
{
    private class MongoDepartment
    {
        public int _id { get; set; }
        public string name { get; set; }
    }

    private class MongoInstitute
    {
        public int _id { get; set; }
        public string name { get; set; }
        public List<MongoDepartment> departments { get; set; } = new();
    }

    private class MongoUniversity
    {
        public int _id { get; set; }
        public string name { get; set; }
        public List<MongoInstitute> institutes { get; set; } = new();
    }

    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<MongoDataSaver> _logger;

    public async Task<SaveResult> SaveAsync(GeneratedData data)
    {
        // --- 4. Сохранение в MongoDB ---
        _logger.LogInformation("Saving university structure to MongoDB...");
        var uniCollection = _mongoDatabase.GetCollection<MongoUniversity>("universities");
        var mongoDocs = new List<MongoUniversity>();

        // Строим вложенную структуру
        foreach (var uni in data.Universities)
        {
            var mongoUni = new MongoUniversity { _id = uni.Id, name = uni.Name };
            var institutesForUni = data.Institutes.Where(i => i.UniversityId == uni.Id).ToList();

            foreach (var inst in institutesForUni)
            {
                var mongoInst = new MongoInstitute { _id = inst.Id, name = inst.Name };
                var departmentsForInst = data.Departments.Where(d => d.InstituteId == inst.Id).ToList();

                foreach (var dept in departmentsForInst)
                {
                    mongoInst.departments.Add(new MongoDepartment { _id = dept.Id, name = dept.Name });
                }

                mongoUni.institutes.Add(mongoInst);
            }

            mongoDocs.Add(mongoUni);
        }

        if (mongoDocs.Any())
        {
            // Удаляем старые документы перед вставкой новых, если нужно перезаписать
            // await uniCollection.DeleteManyAsync(FilterDefinition<MongoUniversity>.Empty);
            await uniCollection.InsertManyAsync(mongoDocs);
            _logger.LogInformation("MongoDB save complete.");
        }
        else
        {
            _logger.LogWarning("No University structure to save to MongoDB.");
        }
        return new SaveResult(true, "University saved to MongoDB.");
    }

    public MongoDataSaver(IMongoDatabase mongoDatabase, ILogger<MongoDataSaver> logger)
    {
        _mongoDatabase = mongoDatabase;
        _logger = logger;
    }
}