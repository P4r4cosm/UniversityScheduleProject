using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Generator;

public class University
{
    public int Id {get; set;}
    [Required] public string Name {get; set;}
}