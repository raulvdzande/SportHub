using SportHub.Shared.DTOs.CheckIns;

namespace SportHub.API.Application.Interfaces;

public interface ICheckInService
{
    Task<CheckInDto> CreateAsync(Guid memberId, CreateCheckInRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CheckInDto>> GetByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CheckInDto>> GetMemberHistoryAsync(Guid memberId, CancellationToken cancellationToken = default);
}
