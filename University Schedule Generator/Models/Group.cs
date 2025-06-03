using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace University_Schedule_Generator;

public class Group
{
    public int Id {get; set;}
    [Required] public string Name { get; set; }
    
    public int DepartmentId { get; set; }
    public Department Department { get; set; }
    
    [Column(TypeName = "date")] // Указываем тип DATE
    public DateTime StartYear { get; set; }
    
    [Column(TypeName = "date")] // Указываем тип DATE
    public DateTime EndYear { get; set; }
}