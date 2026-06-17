namespace SportHub.Shared.DTOs.Reservations;

public class LessonReservationDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid LessonId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public int? WaitlistPosition { get; set; }
    public int? SpinningBikeNumber { get; set; }

    // Denormalised lesson info for display
    public DateTime LessonStartTimeUtc { get; set; }
    public int LessonDurationMinutes { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string? InstructorName { get; set; }
}
