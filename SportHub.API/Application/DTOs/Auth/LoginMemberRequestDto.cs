using System.ComponentModel.DataAnnotations;

namespace SportHub.API.Application.DTOs.Auth;

public class LoginMemberRequestDto
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}