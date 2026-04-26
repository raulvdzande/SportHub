using SportHub.API.Application.DTOs.Workouts;

namespace SportHub.API.Application.Interfaces;

public interface IWorkoutService
{
    Task<IReadOnlyCollection<WorkoutDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WorkoutDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutDto> CreateAsync(CreateWorkoutRequestDto request, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateAsync(Guid id, UpdateWorkoutRequestDto request, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

