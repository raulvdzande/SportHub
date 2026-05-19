namespace SportHub.Shared.DTOs.Lessons;

public class GenerateRecurringLessonsResponseDto
{
    public Guid RecurrenceRuleId { get; set; }
    public int LessonsCreated { get; set; }
    public IReadOnlyList<Guid> LessonIds { get; set; } = Array.Empty<Guid>();
}

