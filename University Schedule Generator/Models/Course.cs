using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Generator;

public class Course
{
    public int Id { get; set; }
    
    [Required]public string Name { get; set; }
    
    public int DepartmentId { get; set; }
    public Department Department { get; set; }
    
    
    public int SpecialityId { get; set; }
    public Speciality Speciality { get; set; }

    // Учебный год/период в формате "ГГГГ-ГГГГ"
    [Required]public string Term{get; set;}
}