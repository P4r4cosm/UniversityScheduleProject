using System.Text;
using University_Schedule_Generator.Infrastructure.Generators.Data;
using University_Schedule_Generator.Interfaces.DataSaver;
using University_Schedule_Generator.Services.DataSavers;

namespace University_Schedule_Generator.Services;

public class DataSaverService
{
    private readonly GeneratorService _generator;
    private readonly ILogger<DataSaverService> _logger;
    private readonly IEnumerable<IDataSaver<GeneratedData>> _dataSavers;

    public DataSaverService(IEnumerable<IDataSaver<GeneratedData>> savers, ILogger<DataSaverService> logger,
        GeneratorService generator)
    {
        _generator = generator;
        _logger = logger;
        _dataSavers = savers;
    }

    public async Task<string> GenerateAndSaveDataAsync(int SpecialtiesCount,
        int UniversityCount,
        int InstitutionCount,
        int DepartmentCount,
        int GroupCount,
        int StudentCount,
        int CourseCount)
    {
        var sbReport = new StringBuilder("Generation and Saving Report:\n");
        try
        {
            var generatedData = _generator.GenerateForPostgres(SpecialtiesCount, UniversityCount, InstitutionCount,
                DepartmentCount, GroupCount, StudentCount, CourseCount);

            //сначала сохраняем в postgres, чтобы не было проблем с id при генерации текстов
            //после во все остальные базы 
            var pgSaver = _dataSavers.OfType<PostgresDataSaver>().FirstOrDefault();
            if (pgSaver == null) throw new InvalidOperationException("PostgresDataSaver not found.");

            var pgResult = await pgSaver.SaveAsync(generatedData);
            generatedData = _generator.GenerateDataForElastic(generatedData);

            var otherSavers = _dataSavers.Where(s => s != pgSaver); // Исключаем уже выполненный pgSaver
            foreach (var saver in otherSavers)
            {
                await saver.SaveAsync(generatedData);
            }

            sbReport.AppendLine("Generation and Saving complete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the process.");
            sbReport.AppendLine($"\n!!! OVERALL ERROR: {ex.Message} !!!");
            return sbReport.ToString();
        }

        return sbReport.ToString();
    }

    private void LogAndAppendResult(StringBuilder sb, string dbName, SaveResult result)
    {
        if (result.Success)
        {
            _logger.LogInformation($"{dbName} save successful. {result.Message}");
            sb.AppendLine($"- Data saved to {dbName}. {result.Message}");
        }
        else
        {
            _logger.LogError(result.Error, $"Error saving to {dbName}: {result.Message}");
            sb.AppendLine($"! Error saving to {dbName}: {result.Message}");
        }
    }
}