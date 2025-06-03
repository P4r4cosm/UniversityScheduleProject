using University_Schedule_Lab1.Contracts;
using University_Schedule_Lab1.Repositories;

namespace University_Schedule_Lab1.Services;

public class FindAudienceService
{
    private readonly ILogger<FindAudienceService> _logger;
    private readonly LectureRepository _lectureRepository;
    private readonly CourseRepository _courseRepository;
    public FindAudienceService(CourseRepository courseRepository, LectureRepository lectureRepository,
        ILogger<FindAudienceService> logger)
    {
       _logger = logger;
       _courseRepository = courseRepository;
       _lectureRepository = lectureRepository;
    }

    public async Task<CourseReportResponse> GetRequirements(string courseName,int year)
    {
        //получаем курс
        var course = _courseRepository.GetCourseByName(courseName);
        //получаем все лекции курса в указанном году
        var lectures = _lectureRepository.GetLecturesByCourseId(course.Id, year);
        //получаем для каждой лекции её группы и кол-во студентов в каждой из групп
        var lectureDict = new Dictionary<Lecture,int>();
        foreach (var lecture in lectures)
        {
           var grouplist = await _lectureRepository.GetGroupsWithStudentCountForLectureAsync(lecture.Id);
           int counter = 0;
           foreach (var group in grouplist)
           {
               counter+=group.StudentCount;
           }
           lectureDict.Add(lecture,counter);
        }
        var LecturesDTO= new List<LectureWithStudentCountDto>();
        foreach (var lecture in lectures)
        {
            LectureWithStudentCountDto lectureDto = new LectureWithStudentCountDto();
            lectureDto.Lecture = lecture;
            lectureDto.StudentCount = lectureDict.ContainsKey(lecture) ? lectureDict[lecture] : 0;
            LecturesDTO.Add(lectureDto);
        }
        
        
        var response = new CourseReportResponse();
        response.Lectures = LecturesDTO;
        response.Course = course;
        
        return response;
    }
}