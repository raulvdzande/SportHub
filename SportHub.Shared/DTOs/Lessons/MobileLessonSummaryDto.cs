namespace SportHub.Shared.DTOs.Lessons;

public class MobileLessonSummaryDto
{
    public Guid Id { get; set; }
    public Guid WorkoutId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public Guid? InstructorId { get; set; }
    public string? InstructorName { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public int DurationMinutes { get; set; }
    public int? CapacityOverride { get; set; }
    public int RegisteredCount { get; set; }
    public bool IsInstructorTbd { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }
}
