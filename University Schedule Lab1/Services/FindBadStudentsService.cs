using University_Schedule_Lab1.Contracts;
using University_Schedule_Lab1.Repositories;

namespace University_Schedule_Lab1.Services;

public class FindBadStudentsService
{
    private readonly ElasticMaterialsRepository _elasticRepository;
    private readonly LectureRepository _lectureRepository;
    private readonly ScheduleRepository _scheduleRepository;
    private readonly ILogger<FindBadStudentsService> _logger;
    private readonly VisitsRepository _visitsRepository;
    private readonly StudentRepository _studentRepository;

    public FindBadStudentsService(ElasticMaterialsRepository repository,
        LectureRepository lectureRepository,
        ScheduleRepository scheduleRepository, ILogger<FindBadStudentsService> logger,
        VisitsRepository visitsRepository,
        StudentRepository studentRepository)
    {
        _elasticRepository = repository;
        _lectureRepository = lectureRepository;
        _scheduleRepository = scheduleRepository;
        _logger = logger;
        _visitsRepository = visitsRepository;
        _studentRepository = studentRepository;
    }

    public async Task<List<LowAttendanceReportResponse>> GetStudents(string searchTerm, DateTime startDate, DateTime endDate)
    {
        var reportItems = new List<LowAttendanceReportResponse>(); // Итоговый список для отчета
        
        //получаем id лекций из elastic-а с заданным словом
        var lecturesIds = await _elasticRepository.GetMaterialElasticByTextAsync(searchTerm, limit:3000);


        var res = await _lectureRepository.GetGroupIdsByLectureId(lecturesIds);
        //получаем id групп и студентов прикриплённых к этим лекциям
        
        var studentIds = res.Item1;
        var groupIds = res.Item2;
        
        //получаем студентов из редиса
        var students = await _studentRepository.GetStudentsByIdsAsync(studentIds);
        var studentsDict = students.ToDictionary(s => s.Id);
        //на основании id group и id лекции находим в postgres расписания лекций для нужных групп
        var schedules = _scheduleRepository.GetSchedulesByLectureAndGroupIds(lecturesIds, groupIds, startDate, endDate);

        //считаем у каждой группы число лекций (ключ - группа, значение - кол-во лекций)
        var lecturesCountsByGroup = schedules
            .GroupBy(s => s.GroupId) // Группируем записи расписания по ID группы
            .ToDictionary(
                g => g.Key, // Ключ словаря - это ключ группировки (GroupId)
                g => g.Count() // Значение словаря - это количество элементов в группе (т.е. кол-во записей расписания/лекций для этой группы)
            );
        //получаем id расписаний
        var scheduleIds = schedules.Select(s => s.Id).ToList();
        //получаем посещения исходя из нужных расписаний
        var visits = _visitsRepository.GetVisitsBySchedule(scheduleIds);
        // Группируем посещения по студентам для быстрого подсчета
        var visitsCountByStudentId = visits
            .GroupBy(v => v.StudentId) // Группируем по студенту
            .ToDictionary(
                g => g.Key, // Ключ - StudentId
                // Значение - количество УНИКАЛЬНЫХ ScheduleId в группе этого студента
                g => g.Select(v => v.ScheduleId).Distinct().Count()
            );

        //Рассчитываем посещаемость для КАЖДОГО студента из списка students 
        var studentAttendanceData = new Dictionary<int, (int Attended, int Total, double Percentage)>();
        foreach (var student in students)
        {
            int studentGroupId = student.GroupId;
            // Считаем, сколько лекций ДОЛЖЕН был посетить студент (из релевантных расписаний для его группы)
            int totalLecturesForStudent = schedules.Count(s => s.GroupId == studentGroupId);

            // Считаем, сколько он ФАКТИЧЕСКИ посетил (из найденных посещений)
            visitsCountByStudentId.TryGetValue(student.Id, out int attendedLecturesCount);

            // Рассчитываем процент
            double attendancePercentage = (totalLecturesForStudent > 0)
                ? ((double)attendedLecturesCount / totalLecturesForStudent) * 100.0
                : 0.0; // Если вдруг total=0 (хотя мы проверили выше), считаем 0%

            studentAttendanceData.Add(student.Id,
                (attendedLecturesCount, totalLecturesForStudent, attendancePercentage));
        }

        //Формируем отчет: сортируем, берем топ-10 с худшей посещаемостью
        var sortedStudents = studentAttendanceData
            .OrderBy(pair => pair.Value.Percentage) // Сортируем по проценту ПО ВОЗРАСТАНИЮ
            .Take(10) // Берем первых 10 (с самой низкой посещаемостью)
            .ToList();
        foreach (var studentDataPair in sortedStudents)
        {
            int studentId = studentDataPair.Key;
            double percentage = studentDataPair.Value.Percentage;

            // Получаем полную информацию о студенте из словаря, созданного ранее
            if (studentsDict.TryGetValue(studentId, out var studentInfo))
            {
                reportItems.Add(new LowAttendanceReportResponse(
                    studentInfo,
                    percentage,
                    startDate,
                    endDate,
                    searchTerm
                ));
            }
            else
            {
                // Этого не должно произойти, если studentAttendanceData строился по students
                _logger.LogError(
                    "Could not find student details in dictionary for Student ID {StudentId} during report finalization.",
                    studentId);
            }
        }

        _logger.LogInformation("Generated report with {Count} items.", reportItems.Count);
        return reportItems;
    }
}