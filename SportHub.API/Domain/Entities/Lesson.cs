namespace SportHub.API.Domain.Entities;

public class Lesson
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

    public Workout Workout { get; set; } = null!;
    public Location Location { get; set; } = null!;
    public Instructor? Instructor { get; set; }
    public LessonRecurrenceRule? RecurrenceRule { get; set; }
    public ICollection<LessonReservation> Reservations { get; set; } = new List<LessonReservation>();
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
}

