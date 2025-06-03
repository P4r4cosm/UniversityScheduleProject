using StackExchange.Redis;
using University_Schedule_Generator.Infrastructure.Generators.Data;
using University_Schedule_Generator.Interfaces.DataSaver;

namespace University_Schedule_Generator.Services.DataSavers;

public class RedisDataSaver : IDataSaver<GeneratedData>
{
    private readonly IDatabase _redis;
    private readonly ILogger<RedisDataSaver> _logger;

    public RedisDataSaver(IDatabase redis, ILogger<RedisDataSaver> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<SaveResult> SaveAsync(GeneratedData data)
    {
        _logger.LogInformation("Saving students to Redis...");
        if (data.Students.Any())
        {
            var redisBatch = _redis.CreateBatch();
            var tasks = new List<Task>();
            foreach (var student in data.Students)
            {
                var key = $"student:{student.Id}";
                var entries = new HashEntry[]
                {
                    new HashEntry("fio", student.FullName),
                    new HashEntry("id_group", student.GroupId),
                    new HashEntry("date_of_recipient", student.DateOfRecipient.ToString("yyyy-MM-dd"))
                };
                // HashSetAsync возвращает Task, который завершается при отправке команды, а не при её выполнении на сервере.
                // Для батча это нормально, т.к. Execute() отправит всё разом.
                tasks.Add(redisBatch.HashSetAsync(key, entries));
            }

            redisBatch.Execute(); // Отправляем батч на сервер
            // Дожидаемся завершения отправки всех команд батча (не обязательно ответа сервера)
            // В большинстве случаев этого достаточно, но для полной гарантии можно использовать Wait или WaitAll на tasks,
            // хотя для HashSetAsync в батче это обычно избыточно.
            await Task.WhenAll(tasks);
            _logger.LogInformation("Redis save complete (batch executed).");
        }
        else
        {
            _logger.LogWarning("No Students to save to Redis.");
        }
        return new SaveResult(true, "Students saved to Redis.");
    }
}