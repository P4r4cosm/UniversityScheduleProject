using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Lab1;

public class Department
{
    public int Id {get; set;}
    [Required] public string Name {get; set;}
    
    
    public int InstituteId {get; set;}
    public Institute Institute {get; set;}
}