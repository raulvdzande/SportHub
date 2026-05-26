namespace SportHub.Shared.DTOs.Lessons;

public class MobileLessonDetailsDto : MobileLessonSummaryDto
{
    public IReadOnlyCollection<LessonParticipantDto> Participants { get; set; } = Array.Empty<LessonParticipantDto>();
}
