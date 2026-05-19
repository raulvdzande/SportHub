using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Members;

public class CreateMemberRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(60)]
    public string? Username { get; set; }

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }
}

