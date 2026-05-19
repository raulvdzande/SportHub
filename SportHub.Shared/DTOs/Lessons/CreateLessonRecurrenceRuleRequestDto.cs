using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Lessons;

public class CreateLessonRecurrenceRuleRequestDto
{
    /// <summary>Daily, Weekly, or Monthly (case-insensitive).</summary>
    [Required]
    public string RecurrenceType { get; set; } = string.Empty;

    [Range(1, 365)]
    public int Interval { get; set; } = 1;

    /// <summary>For Weekly: ISO weekday numbers 1=Monday … 7=Sunday, comma-separated (e.g. "1,3,5").</summary>
    public string? DaysOfWeekCsv { get; set; }

    public DateTime? EndDateUtc { get; set; }

    [Range(1, 500)]
    public int? OccurrenceCount { get; set; }
}

