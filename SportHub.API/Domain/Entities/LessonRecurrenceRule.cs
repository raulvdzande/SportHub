using SportHub.API.Domain.Enums;

namespace SportHub.API.Domain.Entities;

public class LessonRecurrenceRule
{
    public Guid Id { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public int Interval { get; set; } = 1;
    public string? DaysOfWeekCsv { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public int? OccurrenceCount { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

