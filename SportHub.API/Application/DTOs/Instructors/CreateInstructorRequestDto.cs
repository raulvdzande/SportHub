using System.ComponentModel.DataAnnotations;

namespace SportHub.API.Application.DTOs.Instructors;

public class CreateInstructorRequestDto
{
    [Required]
    [StringLength(160)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(1024)]
    public string? PhotoUrl { get; set; }

    public bool IsTbd { get; set; }
    public bool IsActive { get; set; } = true;
}

