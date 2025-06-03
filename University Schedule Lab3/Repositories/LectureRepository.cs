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

    
    public List<Lecture> GetLecturesByCourseId(int courseId, int year)
    {
        var lectures = _context.Lectures.Where(l => l.CourseId == courseId && l.Year == year).ToList();
        return lectures;
    }

    public List<Lecture> GetLecturesById(IEnumerable<int> lectureIds)
    {
        var lectures = _context.Lectures.Where(l => lectureIds.Contains(l.Id)).ToList();
        return lectures;
    }
    // DTO для представления лекции
    public class LectureDto
    {
        public int Id { get; set; } // Используем long для ID из Neo4j
    }

// DTO для представления студента
    public class StudentDto
    {
        public int Id { get; set; } // Используем long для ID из Neo4j
    }

    public class GroupDetailsDto
    {
        public long GroupId { get; set; }
        public List<LectureDto> Lectures { get; set; }
        public List<StudentDto> Students { get; set; }

        public GroupDetailsDto()
        {
            Lectures = new List<LectureDto>();
            Students = new List<StudentDto>();
        }
    }

    public async Task<GroupDetailsDto> GetGroupDetailsAsync(long groupId) // Принимаем long для ID
    {
        _logger.LogInformation("Fetching details for group ID: {GroupId}", groupId);
        var groupDetails = new GroupDetailsDto { GroupId = groupId };

        // Cypher запрос
        // 1. Найти группу по ID.
        // 2. Найти все лекции, связанные с этой группой (через HAS_LECTURE).
        // 3. Найти всех студентов, принадлежащих этой группе (через BELONGS_TO).
        // Используем OPTIONAL MATCH, чтобы вернуть информацию о группе, даже если у нее нет лекций или студентов.
        string cypherQuery = @"
            MATCH (g:Group {id: $groupId})
            OPTIONAL MATCH (g)-[:HAS_LECTURE]->(l:Lecture)
            OPTIONAL MATCH (s:Student)-[:BELONGS_TO]->(g)
            RETURN g.id AS GroupId,
                   g.name AS GroupName, // Если у узла Group есть свойство name
                   collect(DISTINCT {id: l.id, name: l.name}) AS Lectures,
                   collect(DISTINCT {id: s.id, fio: s.fio}) AS Students
        ";
        // Примечание: collect(DISTINCT ...) используется, чтобы избежать дубликатов,
        // если студент или лекция могут быть связаны с группой несколькими путями (хотя по вашей SQL-схеме это маловероятно).
        // Если l или s равны null (из-за OPTIONAL MATCH), то объект {id: null, name: null} может попасть в коллекцию.
        // Мы отфильтруем это в коде.

        var parameters = new { groupId }; // Параметры для запроса

        await using var session = _neo4j.AsyncSession(o => { o.WithDatabase("neo4j"); });

        try
        {
            var resultCursor = await session.RunAsync(cypherQuery, parameters);
            var record = await resultCursor.SingleAsync(); // Ожидаем одну запись, так как ищем одну группу

            if (record != null)
            {
               

                // Обработка лекций
                var lecturesData = record["Lectures"].As<List<IDictionary<string, object>>>();
                foreach (var lecData in lecturesData)
                {
                    // Проверяем, что id не null, чтобы отсеять "пустые" объекты из-за OPTIONAL MATCH
                    if (lecData.TryGetValue("id", out var lectureIdObj) && lectureIdObj != null)
                    {
                        groupDetails.Lectures.Add(new LectureDto
                        {
                            Id = lectureIdObj.As<int>(),
                        });
                    }
                }

                // Обработка студентов
                var studentsData = record["Students"].As<List<IDictionary<string, object>>>();
                foreach (var studData in studentsData)
                {
                    // Проверяем, что id не null
                    if (studData.TryGetValue("id", out var studentIdObj) && studentIdObj != null)
                    {
                        groupDetails.Students.Add(new StudentDto
                        {
                            Id = studentIdObj.As<int>(),
                        });
                    }
                }

                _logger.LogInformation(
                    "Successfully fetched details for group ID: {GroupId}. Lectures: {LectureCount}, Students: {StudentCount}",
                    groupId, groupDetails.Lectures.Count, groupDetails.Students.Count);
            }
            else
            {
                // Этого не должно произойти, если `MATCH (g:Group {id: $groupId})` не нашел группу,
                // тогда SingleAsync() выбросит InvalidOperationException.
                // Но если он вернул null, то группа не найдена.
                _logger.LogWarning("No group found with ID: {GroupId}", groupId);
                return null; // Или выбросить исключение NotFoundException
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Sequence contains no elements"))
        {
            _logger.LogWarning("No group found with ID: {GroupId}", groupId);
            return null; // Группа не найдена
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching group details from Neo4j for group ID: {GroupId}", groupId);
            throw; // Перебрасываем исключение, чтобы вызывающий код мог его обработать
        }

        return groupDetails;
    }


    public List<Lecture> GetLecturesByCourseIdAndLecturesId(IEnumerable<Course> courses, IEnumerable<int> lectureIds)
    {
        var courseIds = courses.Select(c => c.Id).ToList();
        var answer = _context.Lectures
            .Where(l => courseIds.Contains(l.CourseId) && lectureIds.Contains(l.Id))
            .ToList();
        return answer;
    }
}