namespace SportHub.Shared.DTOs.Lessons;

public class LessonParticipantDto
{
    public Guid MemberId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string ReservationStatus { get; set; } = string.Empty;
}
