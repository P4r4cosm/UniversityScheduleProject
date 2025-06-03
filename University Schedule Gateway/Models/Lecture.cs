using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Lab1;

public class Lecture
{
    public int Id {get; set;}
    
    [Required]public string Name {get; set;}
    
    public bool Requirements {get; set;}

    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100")] 
    public int Year { get; set; }
    public int CourseId {get; set;}
    public Course Course {get; set;}
}