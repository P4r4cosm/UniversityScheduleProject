using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Gateway.Models;

public class User
{
    public int Id { get; set; }

    [Required]public string Name { get; set; } = "";
    
    [Required]public string PasswordHash { get; set; } = "";
}