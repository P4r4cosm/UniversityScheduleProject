using System.ComponentModel.DataAnnotations.Schema;

namespace University_Schedule_Lab1;

public class Visit
{
    public int Id { get; set; }
    
    public int StudentId { get; set; }
    public Student Student { get; set; }
    
    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; }
    
    public DateTime VisitTime { get; set; }
}