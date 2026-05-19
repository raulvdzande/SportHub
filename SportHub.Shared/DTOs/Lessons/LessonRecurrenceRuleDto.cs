namespace SportHub.Shared.DTOs.Lessons;

public class LessonRecurrenceRuleDto
{
    public Guid Id { get; set; }
    public string RecurrenceType { get; set; } = string.Empty;
    public int Interval { get; set; }
    public string? DaysOfWeekCsv { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public int? OccurrenceCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

