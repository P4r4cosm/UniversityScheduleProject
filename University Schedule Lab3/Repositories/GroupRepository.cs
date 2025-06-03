using Neo4j.Driver;
using University_Schedule_Lab1.Data;

namespace University_Schedule_Lab1.Repositories;

public class GroupRepository
{
    private readonly ILogger<LectureRepository> _logger;
    private readonly ApplicationContext _context;

    public GroupRepository(ApplicationContext context, IDriver neo4j, ILogger<LectureRepository> logger)
    {
        _logger = logger;
        _context = context;
    }

    public Group GetGroupByName(string groupName)
    {
        return _context.Groups.FirstOrDefault(g => g.Name == groupName);
    }
}