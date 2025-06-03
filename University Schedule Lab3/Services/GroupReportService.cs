using University_Schedule_Lab1.Contracts;
using University_Schedule_Lab1.Repositories;

namespace University_Schedule_Lab1.Services;

public class GroupReportService
{
    private readonly ILogger<GroupReportService> _logger;
    private readonly LectureRepository _lectureRepository;
    private readonly CourseRepository _courseRepository;
    private readonly GroupRepository _groupRepository;
    private readonly StudentRepository _studentRepository;
    private readonly ScheduleRepository _scheduleRepository;
    private readonly VisitsRepository _visitsRepository;
    public GroupReportService(CourseRepository courseRepository, LectureRepository lectureRepository,
        StudentRepository studentRepository,
        GroupRepository groupRepository,
        ScheduleRepository scheduleRepository,
        VisitsRepository visitsRepository,
        ILogger<GroupReportService> logger)
    {
        _logger = logger;
        _courseRepository = courseRepository;
        _lectureRepository = lectureRepository;
        _groupRepository = groupRepository;
        _studentRepository = studentRepository;
        _scheduleRepository = scheduleRepository;
        _visitsRepository = visitsRepository;
    }

    public async Task<GroupReportResponse> GetReport(string groupName)
    {
        //получаем группу по названию
        var group = _groupRepository.GetGroupByName(groupName);
        if (group == null)
        {
            return new GroupReportResponse { Message = "Group not found" };
        }
        _logger.LogInformation($"{groupName}: {group.Id}");
        
        //в neo4j получаем всех связанных с группой студентов и лекции
        var res = await _lectureRepository.GetGroupDetailsAsync(group.Id);
        //id лекций
        var lecturesIds = res.Lectures.Select(l => l.Id).ToList();
        //id студентов
        var studentsIds = res.Students.Select(s => s.Id);
        //получаем студентов из redis
        var students =await _studentRepository.GetStudentsByIdsAsync(studentsIds);
        //получаем все уникальные специальные курсы группы
        var courses = _courseRepository.GetCoursesByLecturesIdsAndDepartmentId(lecturesIds, group.DepartmentId);
        if (!courses.Any())
        {
            return new GroupReportResponse { Message = "No special lectures found for this group" };
        }
        
        //получаем только нужные специальные лекции
        var specialLectures = _lectureRepository.GetLecturesByCourseIdAndLecturesId(courses, lecturesIds);
        //получаем расписания для этой группы всех специальных лекций
        var schedules = _scheduleRepository.GetSchedulesByLecturesIdsAndGroupId(specialLectures, group.Id);
        //считаем все учебные часы специальных лекций
        var AllHours = schedules.Count() * 2;
        //получаем для всех студентов группы их посещения
        var visits = _visitsRepository.GetVisitsByScheduleIdsAndStudentsIds(schedules.Select(s => s.Id), studentsIds);
        
        var studentVisitsCount = visits
            .GroupBy(v => v.StudentId)
            .ToDictionary(g => g.Key, g => g.Count());
        
       
        
        var response = new GroupReportResponse();

        var courseInfo = new CourseDTO();
        courseInfo.courses = courses;
        courseInfo.lectures = specialLectures;
        response.CourseInfo = courseInfo;

        var groupInfo = new GroupDTO();
        groupInfo.group = group;
        var studentsDtoList = new List<StudentDTO>();
        foreach (var student in students)
        {
            var studentDto = new StudentDTO();
            studentDto.student = student;
            studentDto.AllHours = AllHours;
            studentDto.VisitHours = studentVisitsCount.GetValueOrDefault(student.Id, 0) * 2;
            studentsDtoList.Add(studentDto);
        }
        groupInfo.students = studentsDtoList;
        response.GroupInfo = groupInfo;
        return response;
    }
    public string Message { get; set; }
}