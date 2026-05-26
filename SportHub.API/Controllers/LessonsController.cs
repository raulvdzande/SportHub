using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Lessons;
using SportHub.API.Application.Interfaces;

namespace SportHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LessonDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _lessonService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LessonDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _lessonService.GetByIdAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpGet("mobile")]
    public async Task<ActionResult<IReadOnlyCollection<MobileLessonSummaryDto>>> GetMobileSchedule(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var items = await _lessonService.GetMobileScheduleAsync(fromUtc, toUtc, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}/mobile")]
    public async Task<ActionResult<MobileLessonDetailsDto>> GetMobileDetails(Guid id, CancellationToken cancellationToken)
    {
        var item = await _lessonService.GetMobileDetailsAsync(id, cancellationToken);
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<LessonDto>> Create([FromBody] CreateLessonRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _lessonService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LessonDto>> Update(Guid id, [FromBody] UpdateLessonRequestDto request, CancellationToken cancellationToken)
    {
        var updated = await _lessonService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _lessonService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Create a recurrence rule without generating lessons yet.</summary>
    [HttpPost("recurrence-rules")]
    public async Task<ActionResult<LessonRecurrenceRuleDto>> CreateRecurrenceRule(
        [FromBody] CreateLessonRecurrenceRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _lessonService.CreateRecurrenceRuleAsync(request, cancellationToken);
        return Ok(created);
    }

    /// <summary>Create a recurrence rule and generate lesson instances (max 500).</summary>
    [HttpPost("generate-recurring")]
    public async Task<ActionResult<GenerateRecurringLessonsResponseDto>> GenerateRecurring(
        [FromBody] GenerateRecurringLessonsRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _lessonService.GenerateRecurringLessonsAsync(request, cancellationToken);
        return Ok(result);
    }
}
