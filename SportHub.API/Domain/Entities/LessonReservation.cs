using SportHub.API.Domain.Enums;

namespace SportHub.API.Domain.Entities;

public class LessonReservation
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid LessonId { get; set; }
    public LessonReservationStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAtUtc { get; set; }
    public int? WaitlistPosition { get; set; }
    public int? SpinningBikeNumber { get; set; }

    public Member Member { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}

