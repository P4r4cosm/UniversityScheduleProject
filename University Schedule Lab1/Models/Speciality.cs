using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Lab1;

public class Speciality
{
    public int Id {get; set;}
    [Required]public string code {get; set;}
    [Required]public string Name{get; set;} 
}