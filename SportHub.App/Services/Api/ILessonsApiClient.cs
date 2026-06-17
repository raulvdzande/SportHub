using SportHub.Shared.DTOs.Lessons;

namespace SportHub.App.Services.Api;

public interface ILessonsApiClient
{
    Task<IReadOnlyCollection<MobileLessonSummaryDto>> GetMobileScheduleAsync(DateTime fromUtc, DateTime toUtc, Guid? instructorId = null, CancellationToken cancellationToken = default);
    Task<MobileLessonDetailsDto?> GetMobileDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MobileLessonSummaryDto?> CreateAsync(CreateLessonRequestDto request, CancellationToken cancellationToken = default);
    Task<MobileLessonSummaryDto?> UpdateAsync(Guid id, UpdateLessonRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

