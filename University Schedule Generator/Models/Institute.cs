using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Generator;

public class Institute
{
    public int Id {get; set;}
    [Required]public string Name {get; set;}
    
    public int UniversityId {get; set;}
    public University University {get; set;}
}