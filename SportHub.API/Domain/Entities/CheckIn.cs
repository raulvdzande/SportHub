using SportHub.API.Domain.Enums;

namespace SportHub.API.Domain.Entities;

public class CheckIn
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid LessonId { get; set; }
    public CheckInMethod Method { get; set; }
    public DateTime CheckedInAtUtc { get; set; } = DateTime.UtcNow;
    public string? DeviceIdentifier { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public Member Member { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}
