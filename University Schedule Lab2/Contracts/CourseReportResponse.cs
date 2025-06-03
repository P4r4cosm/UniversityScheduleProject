namespace University_Schedule_Lab1.Contracts;

public class CourseReportResponse
{
    public Course Course { get; set; }
    public List<LectureWithStudentCountDto> Lectures { get; set; }
}

