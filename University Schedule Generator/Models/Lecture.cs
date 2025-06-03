using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Generator;

public class Lecture
{
    public int Id {get; set;}
    
    [Required]public string Name {get; set;}
    
    public bool Requirements  {get; set;}
    
    [Range(1900, 2100, ErrorMessage = "Год должен быть в диапазоне от 1900 до 2100")] //  валидация
    public int Year {get; set;}
    public int CourseId {get; set;}
    public Course Course {get; set;}
}