using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace University_Schedule_Generator;

public class Student
{
    public int Id {get; set;}
    
    [Required] public string FullName {get; set;}
    
    [Column(TypeName = "date")] // Указываем тип DATE
    public DateTime DateOfRecipient { get; set; }
    
    public int GroupId { get; set; }
    public Group Group { get; set; }
}