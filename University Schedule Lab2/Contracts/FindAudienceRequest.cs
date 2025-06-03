using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Lab1.Contracts;

public record FindAudienceRequest
{
    [Required]
    [DefaultValue(2025)]
    public int Year { get; set; }
    
    [Required]
    [DefaultValue("Базы данных")]
    public string? CourseName { get; set; }
}