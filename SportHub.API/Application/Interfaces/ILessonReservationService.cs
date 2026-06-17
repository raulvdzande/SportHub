using SportHub.Shared.DTOs.Reservations;

namespace SportHub.API.Application.Interfaces;

public interface ILessonReservationService
{
    Task<LessonReservationDto> CreateAsync(Guid memberId, CreateReservationRequestDto request, CancellationToken cancellationToken = default);
    Task<LessonReservationDto> CancelAsync(Guid reservationId, Guid memberId, CancellationToken cancellationToken = default);
    Task<Guid?> RemoveParticipantAsync(Guid lessonId, Guid memberId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LessonReservationDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LessonReservationDto>> GetByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);
    Task<LessonReservationDto?> GetMemberReservationForLessonAsync(Guid memberId, Guid lessonId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>> GetTakenBikesAsync(Guid lessonId, CancellationToken cancellationToken = default);
}
