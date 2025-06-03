using Neo4j.Driver;
using Microsoft.Extensions.Logging; // Используем стандартный логгер
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using University_Schedule_Lab1.Data;

namespace University_Schedule_Lab1.Repositories;

public class LectureRepository
{
    private readonly IDriver _neo4j;
    private readonly ILogger<LectureRepository> _logger;
    private readonly ApplicationContext _context;

    public LectureRepository(ApplicationContext context, IDriver neo4j, ILogger<LectureRepository> logger)
    {
        _neo4j = neo4j;
        _logger = logger;
        _context = context;
    }


    /// <summary>
    /// Получает списки ID студентов и групп, связанных с заданными лекциями.
    /// Использует связи [:CAN_ATTEND] для студентов и [:HAS_LECTURE] для групп.
    /// </summary>
    /// <param name="lectureIds">Перечисление ID лекций.</param>
    /// <returns>Кортеж с двумя списками: List<int> studentIdList и List<int> groupIdList.</returns>
    public async Task<(List<int> studentIdList, List<int> groupIdList)> GetStudentsAndGroupsForLecturesAsync(
        IEnumerable<int> lectureIds)
    {
        // Инициализируем пустые списки на случай ошибки или отсутствия данных
        var studentIdList = new List<int>();
        var groupIdList = new List<int>();

        if (lectureIds == null || !lectureIds.Any())
        {
            _logger.LogWarning("Input lectureIds collection is null or empty.");
            return (studentIdList, groupIdList); // Возвращаем пустые списки
        }

        // Преобразуем ID в long для Neo4j и создаем параметры
        var parameters = new { LectureIds = lectureIds.Select(id => (long)id).Distinct().ToList() };

        // Запрос Cypher для получения ID студентов (через CAN_ATTEND) и групп (через HAS_LECTURE)
        // Используем OPTIONAL MATCH на случай, если у лекции нет студентов или групп
        // Используем collect(DISTINCT ...) для сбора всех уникальных ID в один список для каждой категории
        string cypherQuery = @"
            MATCH (l:Lecture)
            WHERE l.id IN $LectureIds
            OPTIONAL MATCH (s:Student)-[:CAN_ATTEND]->(l)  // Используем новую прямую связь для студентов
            OPTIONAL MATCH (g:Group)-[:HAS_LECTURE]->(l)   // Используем связь для групп
            RETURN collect(DISTINCT s.id) AS StudentIds, collect(DISTINCT g.id) AS GroupIds";

        await using var
            session = _neo4j.AsyncSession(o => o.WithDatabase("neo4j")); // Укажите вашу БД, если она не neo4j

        try
        {
            _logger.LogDebug("Executing Cypher query to get students and groups for lectures: {LectureIds}",
                string.Join(",", parameters.LectureIds));
            var resultCursor = await session.RunAsync(cypherQuery, parameters);

            // collect() гарантирует, что всегда будет ровно одна запись в результате
            var record = await resultCursor.SingleAsync();

            // Извлекаем списки ID из записи
            // Драйвер Neo4j возвращает списки как List<object>, содержащие long
            var studentIdObjects = record["StudentIds"]?.As<List<object>>(); // Безопасное извлечение
            var groupIdObjects = record["GroupIds"]?.As<List<object>>(); // Безопасное извлечение

            // Конвертируем List<object> (содержащий long) в List<int>
            if (studentIdObjects != null)
            {
                studentIdList = studentIdObjects
                    .Where(obj => obj != null) // На случай если в collect попал null (маловероятно с ID)
                    .Select(obj => Convert.ToInt32(obj)) // Преобразуем long в int
                    .ToList();
                _logger.LogDebug("Found {Count} student IDs.", studentIdList.Count);
            }
            else
            {
                _logger.LogDebug("No student IDs found in the result record.");
            }


            if (groupIdObjects != null)
            {
                groupIdList = groupIdObjects
                    .Where(obj => obj != null)
                    .Select(obj => Convert.ToInt32(obj)) // Преобразуем long в int
                    .ToList();
                _logger.LogDebug("Found {Count} group IDs.", groupIdList.Count);
            }
            else
            {
                _logger.LogDebug("No group IDs found in the result record.");
            }
        }
        catch (Exception ex)
        {
            // Используем логгер вместо Console.WriteLine
            _logger.LogError(ex, "Error getting students and groups from Neo4j for lectures {LectureIds}",
                string.Join(",", parameters.LectureIds));
            // В случае ошибки возвращаем пустые списки (уже инициализированы)
        }

        return (studentIdList, groupIdList);
    }

    // Можно оставить старое имя метода как обертку, если это нужно для совместимости,
    // но лучше переименовать вызовы на новый асинхронный метод с более ясным именем.
    public Task<(List<int> studentIdList, List<int> groupIdList)> GetGroupIdsByLectureId(IEnumerable<int> LectureIds)
    {
        // Просто вызываем новый асинхронный метод
        return GetStudentsAndGroupsForLecturesAsync(LectureIds);
    }

    public List<Lecture> GetLecturesByCourseId(int courseId, int year)
    {
        var lectures = _context.Lectures.Where(l => l.CourseId == courseId && l.Year == year).ToList();
        return lectures;
    }
    

    /// <summary>
    /// Получает список групп, связанных с указанной лекцией,
    /// и количество студентов в каждой из этих групп.
    /// </summary>
    /// <param name="lectureId">ID лекции.</param>
    /// <returns>Список DTO GroupStudentCountDto для групп, связанных с лекцией.</returns>
    public async Task<List<GroupStudentCountDto>> GetGroupsWithStudentCountForLectureAsync(int lectureId)
    {
        var results = new List<GroupStudentCountDto>();
        var parameters = new { LectureId = (long)lectureId }; // Преобразуем int в long для Neo4j

        // Запрос Cypher:
        // 1. Найти лекцию по её ID.
        // 2. Найти все группы, связанные с этой лекцией через HAS_LECTURE.
        // 3. Для каждой найденной группы посчитать студентов, связанных через BELONGS_TO.
        // Используем OPTIONAL MATCH для студентов, чтобы группы без студентов тоже попали в результат.
        string cypherQuery = @"
            MATCH (l:Lecture {id: $LectureId}) // Находим конкретную лекцию по ID
            MATCH (g:Group)-[:HAS_LECTURE]->(l) // Находим группы, связанные с ЭТОЙ лекцией
            WITH g                              // Передаем найденные группы дальше
            OPTIONAL MATCH (s:Student)-[:BELONGS_TO]->(g) // Ищем студентов для каждой из этих групп
            RETURN g.id AS GroupId,               // Возвращаем ID группы
                   count(s) AS StudentCount       // Считаем студентов для этой группы
            ORDER BY GroupId           // Опциональная сортировка
        ";
        // Примечание: `WITH DISTINCT g` здесь не нужен, так как мы начинаем с одной лекции,
        // и если модель связи Group-[:HAS_LECTURE]->Lecture уникальна, то каждая группа найдется один раз.
        // Если же возможны дубликаты связей, то `WITH DISTINCT g` можно вернуть.

        await using var session = _neo4j.AsyncSession(o => o.WithDatabase("neo4j")); // Укажите вашу БД, если нужно

        try
        {
            _logger.LogDebug("Executing Cypher query to get groups and student counts for lecture ID: {LectureId}",
                lectureId);
            var resultCursor = await session.RunAsync(cypherQuery, parameters);

            var records = await resultCursor.ToListAsync();

            if (!records.Any())
            {
                _logger.LogDebug("No groups found associated with lecture ID: {LectureId}.", lectureId);
                return results; // Возвращаем пустой список
            }

            _logger.LogDebug("Processing {Count} group records found for lecture ID: {LectureId}.", records.Count,
                lectureId);

            foreach (var record in records)
            {
                results.Add(new GroupStudentCountDto
                {
                    GroupId = record["GroupId"] != null ? Convert.ToInt32(record["GroupId"]) : 0,
                    StudentCount = record["StudentCount"] != null ? Convert.ToInt32(record["StudentCount"]) : 0
                });
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Result contains no more records"))
        {
            // Это нормальный случай, если лекция существует, но к ней не привязано ни одной группы
            _logger.LogDebug("Query completed, but no groups found for lecture ID: {LectureId}.", lectureId);
            // results останется пустым
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups and student counts from Neo4j for lecture ID: {LectureId}",
                lectureId);
            // В случае ошибки можно вернуть пустой список или перебросить исключение
            // return new List<GroupStudentCountDto>();
        }

        return results;
    }
}