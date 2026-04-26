using Microsoft.EntityFrameworkCore;
using SportHub.API.Application.DTOs.Workouts;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class WorkoutService : IWorkoutService
{
    private readonly AppDbContext _dbContext;

    public WorkoutService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<WorkoutDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workouts
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new WorkoutDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                DefaultDurationMinutes = x.DefaultDurationMinutes,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkoutDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _dbContext.Workouts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Workout not found.");

        return MapToDto(workout);
    }

    public async Task<WorkoutDto> CreateAsync(CreateWorkoutRequestDto request, CancellationToken cancellationToken = default)
    {
        await EnsureUniqueNameAsync(request.Name, cancellationToken);

        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Workouts.Add(workout);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(workout);
    }

    public async Task<WorkoutDto> UpdateAsync(Guid id, UpdateWorkoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var workout = await _dbContext.Workouts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Workout not found.");

        await EnsureUniqueNameAsync(request.Name, cancellationToken, id);

        workout.Name = request.Name.Trim();
        workout.Description = request.Description?.Trim();
        workout.DefaultDurationMinutes = request.DefaultDurationMinutes;
        workout.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(workout);
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _dbContext.Workouts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Workout not found.");

        var deletedLessonCount = await _dbContext.Lessons.CountAsync(x => x.WorkoutId == id, cancellationToken);

        _dbContext.Workouts.Remove(workout);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return deletedLessonCount;
    }

    private async Task EnsureUniqueNameAsync(string name, CancellationToken cancellationToken, Guid? excludeId = null)
    {
        var normalized = name.Trim().ToLowerInvariant();

        var exists = await _dbContext.Workouts.AnyAsync(
            x => x.Name.ToLower() == normalized && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("A workout with this name already exists.");
        }
    }

    private static WorkoutDto MapToDto(Workout workout)
    {
        return new WorkoutDto
        {
            Id = workout.Id,
            Name = workout.Name,
            Description = workout.Description,
            DefaultDurationMinutes = workout.DefaultDurationMinutes,
            IsActive = workout.IsActive,
            CreatedAtUtc = workout.CreatedAtUtc
        };
    }
}
