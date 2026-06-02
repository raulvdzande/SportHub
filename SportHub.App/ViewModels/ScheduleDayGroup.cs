using System.Collections.Generic;
using SportHub.Shared.DTOs.Lessons;

namespace SportHub.App.ViewModels;

public class ScheduleDayGroup
{
    public string Date { get; set; } = string.Empty;
    public IReadOnlyCollection<MobileLessonSummaryDto> Lessons { get; set; } = Array.Empty<MobileLessonSummaryDto>();

    public ScheduleDayGroup() { }

    public ScheduleDayGroup(string date, IReadOnlyCollection<MobileLessonSummaryDto> lessons)
    {
        Date = date;
        Lessons = lessons;
    }
}

