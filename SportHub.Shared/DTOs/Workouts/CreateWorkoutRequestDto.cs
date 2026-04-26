using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Workouts;

public class CreateWorkoutRequestDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(5, 480)]
    public int DefaultDurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;
}

