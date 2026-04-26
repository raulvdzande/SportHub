using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Workouts;
using SportHub.API.Application.Interfaces;

namespace SportHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _workoutService;

    public WorkoutsController(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<WorkoutDto>>> GetAll(CancellationToken cancellationToken)
    {
        var workouts = await _workoutService.GetAllAsync(cancellationToken);
        return Ok(workouts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkoutDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var workout = await _workoutService.GetByIdAsync(id, cancellationToken);
        return Ok(workout);
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutDto>> Create([FromBody] CreateWorkoutRequestDto request, CancellationToken cancellationToken)
    {
        var workout = await _workoutService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = workout.Id }, workout);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkoutDto>> Update(Guid id, [FromBody] UpdateWorkoutRequestDto request, CancellationToken cancellationToken)
    {
        var workout = await _workoutService.UpdateAsync(id, request, cancellationToken);
        return Ok(workout);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<DeleteWorkoutResponseDto>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deletedLessonCount = await _workoutService.DeleteAsync(id, cancellationToken);
        return Ok(new DeleteWorkoutResponseDto
        {
            Message = "Workout and related lessons were deleted.",
            DeletedLessons = deletedLessonCount
        });
    }
}
