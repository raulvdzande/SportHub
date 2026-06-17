using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Auth;

public class LoginInstructorRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
