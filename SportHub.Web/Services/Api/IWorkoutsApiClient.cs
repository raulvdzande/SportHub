using SportHub.Shared.DTOs.Workouts;

namespace SportHub.Web.Services.Api;

public interface IWorkoutsApiClient
{
    Task<IReadOnlyCollection<WorkoutDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WorkoutDto> CreateAsync(CreateWorkoutRequestDto request, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateAsync(Guid id, UpdateWorkoutRequestDto request, CancellationToken cancellationToken = default);
    Task<DeleteWorkoutResponseDto?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

