using System.ComponentModel.DataAnnotations;

namespace University_Schedule_Gateway.Contracts;

public record RegisterUserRequest(
    [Required] string Name,
    [Required] string Password);