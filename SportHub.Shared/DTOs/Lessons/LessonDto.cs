namespace SportHub.Shared.DTOs.Lessons;

public class LessonDto
{
    public Guid Id { get; set; }
    public Guid WorkoutId { get; set; }
    public Guid LocationId { get; set; }
    public Guid? InstructorId { get; set; }
    public Guid? RecurrenceRuleId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public int DurationMinutes { get; set; }
    public int? CapacityOverride { get; set; }
    public bool IsInstructorTbd { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }
}

