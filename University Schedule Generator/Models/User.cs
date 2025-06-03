using System.ComponentModel.DataAnnotations;


public class User
{
    public int Id { get; set; }

    [Required]public string Name { get; set; } = "";
    
    [Required]public string PasswordHash { get; set; } = "";
}