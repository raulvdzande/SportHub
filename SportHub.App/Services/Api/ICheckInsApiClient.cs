using SportHub.Shared.DTOs.CheckIns;

namespace SportHub.App.Services.Api;

public interface ICheckInsApiClient
{
    Task<CheckInDto?> CreateAsync(CreateCheckInRequestDto request, CancellationToken ct = default);
    Task<IReadOnlyCollection<CheckInDto>?> GetByLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task<IReadOnlyCollection<CheckInDto>?> GetMemberHistoryAsync(CancellationToken ct = default);
}
