using Neo4j.Driver;
using University_Schedule_Generator.Infrastructure.Generators.Data;
using University_Schedule_Generator.Interfaces.DataSaver;
using Microsoft.Extensions.Logging; // Убедитесь, что используете Microsoft.Extensions.Logging
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace University_Schedule_Generator.Services.DataSavers;

public class Neo4jDataSaver : IDataSaver<GeneratedData>
{
    private readonly IDriver _neo4j;
    private readonly ILogger<Neo4jDataSaver> _logger;

    public Neo4jDataSaver(IDriver neo4j, ILogger<Neo4jDataSaver> logger)
    {
        _neo4j = neo4j;
        _logger = logger;
    }

    public async Task<SaveResult> SaveAsync(GeneratedData data)
    {
        _logger.LogInformation("Saving structural graph data (IDs and relationships including direct Student-Lecture) to Neo4j...");
        await using var session = _neo4j.AsyncSession(o => o.WithDatabase("neo4j"));

        try
        {
            // Очистка графа (если нужно)
            // ... (код очистки) ...

            // 1. Создание/обновление узлов ТОЛЬКО с ID
            await session.ExecuteWriteAsync(async tx =>
            {
                _logger.LogDebug("Merging Neo4j Nodes (ID only)...");
                if (data.Lectures?.Any() ?? false)
                {
                    var lecturesParams = data.Lectures.Select(l => new { id = l.Id }).ToList();
                    await tx.RunAsync("UNWIND $batch AS props MERGE (:Lecture {id: props.id})", new { batch = lecturesParams });
                }
                if (data.Groups?.Any() ?? false)
                {
                    var groupsParams = data.Groups.Select(g => new { id = g.Id }).ToList();
                    await tx.RunAsync("UNWIND $batch AS props MERGE (:Group {id: props.id})", new { batch = groupsParams });
                }
                if (data.Students?.Any() ?? false)
                {
                    var studentsParams = data.Students.Select(s => new { id = s.Id }).ToList();
                    await tx.RunAsync("UNWIND $batch AS props MERGE (:Student {id: props.id})", new { batch = studentsParams });
                }
                _logger.LogDebug("Neo4j Nodes merged (ID only).");
            });

            // 2. Создание связей Группа -> Лекция (HAS_LECTURE)
            await session.ExecuteWriteAsync(async tx =>
            {
                _logger.LogDebug("Merging Neo4j HAS_LECTURE relationships...");
                if (data.Schedules?.Any() ?? false)
                {
                    var scheduleParams = data.Schedules
                        .Select(sch => new { groupId = sch.GroupId, lectureId = sch.LectureId })
                        .Distinct()
                        .ToList();
                    if (scheduleParams.Any())
                    {
                        await tx.RunAsync(@"
                            UNWIND $batch AS props
                            MATCH (g:Group {id: props.groupId})
                            MATCH (l:Lecture {id: props.lectureId})
                            MERGE (g)-[:HAS_LECTURE]->(l)",
                            new { batch = scheduleParams });
                        _logger.LogDebug("Neo4j HAS_LECTURE relationships merged.");
                    }
                }
                 else { _logger.LogInformation("No schedule data, skipping HAS_LECTURE."); }
            });

            // 3. Создание связей Студент -> Группа (BELONGS_TO)
            await session.ExecuteWriteAsync(async tx =>
            {
                _logger.LogDebug("Merging Neo4j BELONGS_TO relationships...");
                if (data.Students?.Any(s => s.GroupId > 0) ?? false)
                {
                    var studentGroupParams = data.Students
                        .Where(s => s.GroupId > 0)
                        .Select(s => new { studentId = s.Id, groupId = s.GroupId })
                        .Distinct()
                        .ToList();
                    if (studentGroupParams.Any())
                    {
                        await tx.RunAsync(@"
                            UNWIND $batch AS props
                            MATCH (s:Student {id: props.studentId})
                            MATCH (g:Group {id: props.groupId})
                            MERGE (s)-[:BELONGS_TO]->(g)",
                            new { batch = studentGroupParams });
                        _logger.LogDebug("Neo4j BELONGS_TO relationships merged.");
                    }
                }
                 else { _logger.LogInformation("No student data with GroupId, skipping BELONGS_TO."); }
            });

            // 4. Создание прямых связей Студент -> Лекция (CAN_ATTEND)
            // Эта связь выводится из существующих данных: Студент принадлежит Группе, Группе назначена Лекция.
            await session.ExecuteWriteAsync(async tx =>
            {
                _logger.LogDebug("Merging direct Neo4j CAN_ATTEND relationships (Student -> Lecture)...");

                // Проверяем, есть ли вообще студенты с группами и расписание
                if ((data.Students?.Any(s => s.GroupId > 0) ?? false) && (data.Schedules?.Any() ?? false))
                {
                    // Шаг 4.1: Создаем удобную структуру для поиска лекций по группе
                    var groupToLectureIdsMap = data.Schedules
                        .GroupBy(s => s.GroupId)
                        .ToDictionary(g => g.Key, g => g.Select(s => s.LectureId).Distinct().ToList());

                    // Шаг 4.2: Генерируем параметры для связи Student-Lecture
                    var canAttendParams = data.Students
                        // Берем студентов с валидной группой, для которой есть лекции в карте
                        .Where(s => s.GroupId > 0 && groupToLectureIdsMap.ContainsKey(s.GroupId))
                        // Для каждого такого студента, берем все лекции его группы
                        .SelectMany(s => groupToLectureIdsMap[s.GroupId]
                            // И создаем пару (StudentId, LectureId)
                            .Select(lectureId => new { studentId = s.Id, lectureId = lectureId }))
                        .Distinct() // Убираем дубликаты пар (StudentId, LectureId)
                        .ToList();

                    // Шаг 4.3: Создаем связи в Neo4j
                    if (canAttendParams.Any())
                    {
                        await tx.RunAsync(@"
                            UNWIND $batch AS props
                            MATCH (s:Student {id: props.studentId})
                            MATCH (l:Lecture {id: props.lectureId})
                            MERGE (s)-[:CAN_ATTEND]->(l)", // Новая прямая связь
                            new { batch = canAttendParams });
                        _logger.LogDebug("Neo4j CAN_ATTEND relationships merged.");
                    }
                    else
                    {
                         _logger.LogDebug("No Student-Lecture pairs derived for CAN_ATTEND relationship.");
                    }
                }
                else
                {
                     _logger.LogInformation("Insufficient data (Students with groups or Schedules) to create CAN_ATTEND relationships.");
                }
            });


            _logger.LogInformation("Neo4j structural graph data (IDs and relationships including direct Student-Lecture) saved successfully.");
            return new SaveResult(true, "Neo4j structural data (IDs and relationships including direct Student-Lecture) saved.");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save structural data to Neo4j.");
            return new SaveResult(false, $"Failed to save to Neo4j: {ex.Message}");
        }
    }
}