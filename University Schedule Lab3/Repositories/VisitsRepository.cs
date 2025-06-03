using University_Schedule_Lab1.Data;

namespace University_Schedule_Lab1.Repositories;

public class VisitsRepository
{
    private readonly ApplicationContext _context;
    private readonly ILogger<VisitsRepository> _logger;

    public VisitsRepository(ApplicationContext context, ILogger<VisitsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<Visit> GetVisitsBySchedule(IEnumerable<int> schedulesIds)
    {
        return _context.Visits.Where(v => schedulesIds.Contains(v.ScheduleId)).ToList();
    }

    public List<Visit> GetVisitsByScheduleIdsAndStudentsIds(IEnumerable<int> schedulesIds, IEnumerable<int> studentsIds)
    {
        var answer = _context.Visits.Where(v => schedulesIds.Contains(v.ScheduleId) && studentsIds.Contains(v.StudentId)).ToList();
        return answer;
    }

}