using StackExchange.Redis;
using University_Schedule_Lab1.Data; // Или где находится ваш класс Student
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization; // Для ParseExact

namespace University_Schedule_Lab1.Repositories;

public class StudentRepository
{
    private readonly IDatabase _database;
    private readonly ILogger<StudentRepository> _logger;
    private const string StudentKeyPrefix = "student:";
    // Определим ожидаемый формат даты для надежности
    private const string DateFormat = "yyyy-MM-dd";

    public StudentRepository(IDatabase database, ILogger<StudentRepository> logger)
    {
        _logger = logger;
        _database = database;
    }

    // Основной асинхронный метод
    public async Task<List<Student>> GetStudentsByIdsAsync(IEnumerable<int> ids)
    {
        var students = new List<Student>();
        if (ids == null || !ids.Any())
        {
            _logger.LogInformation("Input IDs list is null or empty.");
            return students;
        }

        var distinctIds = ids.Distinct().ToList();
        var tasks = new List<Task<(int Id, HashEntry[] Data)>>(distinctIds.Count);

        _logger.LogInformation("Creating async tasks to fetch student data for IDs: [{StudentIds}]", string.Join(",", distinctIds));

        // Запускаем все запросы HGETALL асинхронно
        foreach (var id in distinctIds)
        {
            var redisKey = (RedisKey)$"{StudentKeyPrefix}{id}";
            tasks.Add(FetchStudentDataAsync(id, redisKey));
        }

        _logger.LogInformation("Waiting for {TaskCount} HGETALL tasks to complete.", tasks.Count);

        // Ожидаем завершения всех задач
        var results = await Task.WhenAll(tasks);

        _logger.LogInformation("All fetch tasks completed. Processing results...");

        // Обрабатываем результаты
        foreach (var (id, hashEntries) in results)
        {
            if (hashEntries != null && hashEntries.Length > 0)
            {
                // Десериализуем данные с учетом известных полей
                var student = DeserializeStudent(hashEntries, id);
                if (student != null)
                {
                    students.Add(student);
                }
                // Ошибка десериализации уже залогирована внутри DeserializeStudent
            }
            else
            {
                 // Студент не найден или произошла ошибка при получении данных
                 _logger.LogWarning("No data found or error occurred for student ID: {StudentId}", id);
            }
        }

        _logger.LogInformation("Successfully retrieved and processed {StudentCount} students out of {RequestedCount} distinct IDs.", students.Count, distinctIds.Count);
        return students;
    }

    // Вспомогательный метод для получения данных одного студента
    private async Task<(int Id, HashEntry[] Data)> FetchStudentDataAsync(int id, RedisKey key)
    {
        try
        {
            // HashGetAllAsync возвращает пустой массив, если ключ не найден, а не null
            var data = await _database.HashGetAllAsync(key);
            return (id, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from Redis for key {RedisKey} (ID: {StudentId})", key, id);
            // Возвращаем null для данных, чтобы сигнализировать об ошибке выше
            return (id, null);
        }
    }

    // Десериализация с учетом известных полей
    private Student? DeserializeStudent(HashEntry[] hashEntries, int id)
    {
        // hashEntries не должен быть null, если FetchStudentDataAsync отработал без ошибок,
        // но проверка на пустой массив полезна (если ключ существует, но хэш пуст)
        if (hashEntries == null || hashEntries.Length == 0)
        {
             _logger.LogWarning("Hash data for student ID {StudentId} is null or empty, cannot deserialize.", id);
             return null;
        }

        try
        {
            // Преобразуем в словарь для удобного доступа (игнорируя регистр имен полей)
            var properties = hashEntries.ToDictionary(
                he => he.Name.ToString(), // Ключ - имя поля (string)
                he => he.Value,          // Значение - RedisValue
                StringComparer.OrdinalIgnoreCase); // Сравнение без учета регистра

            var student = new Student { Id = id };
            bool success = true;

            // Извлекаем ФИО ("fio")
            if (properties.TryGetValue("fio", out var fioValue) && !fioValue.IsNullOrEmpty)
            {
                student.FullName = fioValue.ToString();
            }
            else
            {
                _logger.LogWarning("Field 'fio' is missing or empty for student ID {StudentId}", id);
                // Решите, является ли это критической ошибкой. Пока продолжаем.
                // success = false; // Раскомментируйте, если ФИО обязательно
            }

            // Извлекаем ID группы ("id_group")
            if (properties.TryGetValue("id_group", out var groupIdValue) && groupIdValue.TryParse(out int groupId))
            {
                student.GroupId = groupId;
            }
            else
            {
                _logger.LogError("Field 'id_group' is missing or failed to parse as int for student ID {StudentId}. Value: '{Value}'", id, groupIdValue);
                success = false; // Считаем это критической ошибкой
            }

            // Извлекаем дату ("date_of_recipient")
            if (properties.TryGetValue("date_of_recipient", out var dateValue) &&
                DateTime.TryParseExact(dateValue.ToString(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateOfRecipient))
            {
                student.DateOfRecipient = dateOfRecipient;
            }
            else
            {
                 _logger.LogError("Field 'date_of_recipient' is missing or failed to parse with format '{Format}' for student ID {StudentId}. Value: '{Value}'", DateFormat, id, dateValue);
                 success = false; // Считаем это критической ошибкой
            }

            if (success)
            {
                return student;
            }
            else
            {
                 _logger.LogError("Failed to fully deserialize student data for ID {StudentId} due to missing or invalid fields.", id);
                 return null; // Возвращаем null, если не удалось разобрать критические поля
            }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Unexpected error during deserialization for student ID {StudentId}", id);
             return null; // Возвращаем null в случае любой другой ошибки
        }
    }

    // Синхронная обертка (если нужна)
    public List<Student> GetStudentsByIds(IEnumerable<int> ids)
    {
        // Осторожно с .GetAwaiter().GetResult() в некоторых средах
        return GetStudentsByIdsAsync(ids).GetAwaiter().GetResult();
    }
}