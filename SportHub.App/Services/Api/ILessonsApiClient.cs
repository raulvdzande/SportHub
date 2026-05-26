using SportHub.Shared.DTOs.Lessons;

namespace SportHub.App.Services.Api;

public interface ILessonsApiClient
{
    Task<IReadOnlyCollection<MobileLessonSummaryDto>> GetMobileScheduleAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<MobileLessonDetailsDto?> GetMobileDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}

