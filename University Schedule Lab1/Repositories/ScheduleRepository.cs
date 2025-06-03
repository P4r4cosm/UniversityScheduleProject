using University_Schedule_Lab1.Data;

namespace University_Schedule_Lab1.Repositories;

public class ScheduleRepository
{
    private readonly ApplicationContext _context;
    private readonly ILogger<ScheduleRepository> _logger;

    public ScheduleRepository(ApplicationContext context, ILogger<ScheduleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<Schedule> GetSchedulesByLectureAndGroupIds(IEnumerable<int> lecturesIds, IEnumerable<int> groupIds,  DateTime startDate, DateTime endDate)
    {
        var res=  _context.Schedules
            .Where(s=>lecturesIds.Contains(s.LectureId) && groupIds.Contains(s.GroupId))
            .Where(s => s.StartTime >= startDate.ToUniversalTime() && s.StartTime <= endDate.ToUniversalTime())
            .ToList();
        return res;
    }
}