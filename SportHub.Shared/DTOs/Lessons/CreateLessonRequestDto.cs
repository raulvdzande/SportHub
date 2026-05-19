using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Lessons;

public class CreateLessonRequestDto
{
    [Required]
    public Guid WorkoutId { get; set; }

    [Required]
    public Guid LocationId { get; set; }

    public Guid? InstructorId { get; set; }

    public Guid? RecurrenceRuleId { get; set; }

    [Required]
    public DateTime StartTimeUtc { get; set; }

    [Range(1, 24 * 60)]
    public int DurationMinutes { get; set; }

    public int? CapacityOverride { get; set; }

    public bool IsInstructorTbd { get; set; }
}

