namespace SportHub.Shared.DTOs.CheckIns;

public class CheckInDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid LessonId { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime CheckedInAtUtc { get; set; }
    public string? DeviceIdentifier { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Denormalised for display
    public string WorkoutName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public DateTime LessonStartTimeUtc { get; set; }

    // Member info (for instructor view)
    public string DisplayName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}
