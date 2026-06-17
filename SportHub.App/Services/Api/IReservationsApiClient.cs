using SportHub.Shared.DTOs.Reservations;

namespace SportHub.App.Services.Api;

public interface IReservationsApiClient
{
    Task<IReadOnlyCollection<LessonReservationDto>?> GetMyReservationsAsync(CancellationToken ct = default);
    Task<LessonReservationDto?> CreateAsync(CreateReservationRequestDto request, CancellationToken ct = default);
    Task<LessonReservationDto?> CancelAsync(Guid reservationId, CancellationToken ct = default);
    Task<Guid?> RemoveParticipantAsync(Guid lessonId, Guid memberId, CancellationToken ct = default);
    Task<IReadOnlyCollection<LessonReservationDto>?> GetByLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task<IReadOnlyCollection<int>?> GetTakenBikesAsync(Guid lessonId, CancellationToken ct = default);
}
