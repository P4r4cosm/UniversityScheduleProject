using Microsoft.EntityFrameworkCore;
using University_Schedule_Lab1.Data;

namespace University_Schedule_Lab1.Repositories;

public class CourseRepository
{
    private readonly ApplicationContext _context;
    private readonly ILogger<CourseRepository> _logger;

    public CourseRepository(ApplicationContext context, ILogger<CourseRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public  Course? GetCourseByName(string courseName)
    {
        if (string.IsNullOrEmpty(courseName))
            return null;
        return _context.Courses
            .FirstOrDefault(c=> c != null && EF.Functions.Like(c.Name, courseName));
    }
    
    public List<Course> GetCoursesByLecturesIdsAndDepartmentId(List<int> lectureIds, int departmentId)
    {
        return _context.Lectures
            .Where(l => lectureIds.Contains(l.Id))
            .Select(l => l.Course)
            .Where(c => c.DepartmentId == departmentId)
            .Distinct()
            .ToList();
    }
}