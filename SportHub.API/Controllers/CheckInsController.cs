using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.API.Application.Interfaces;
using SportHub.Shared.DTOs.CheckIns;
using System.Security.Claims;

namespace SportHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckInsController : ControllerBase
{
    private readonly ICheckInService _service;
    private readonly ILogger<CheckInsController> _logger;

    public CheckInsController(ICheckInService service, ILogger<CheckInsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Create check-in (Rfid, Gps, or Manual).</summary>
    [HttpPost]
    public async Task<ActionResult<CheckInDto>> Create(CreateCheckInRequestDto request, CancellationToken cancellationToken)
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(memberId, out var id))
            return Unauthorized();

        var result = await _service.CreateAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetMemberHistory), new { }, result);
    }

    /// <summary>Get all check-ins for a lesson (for instructors).</summary>
    [HttpGet("lesson/{lessonId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<CheckInDto>>> GetByLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByLessonAsync(lessonId, cancellationToken));
    }

    /// <summary>Get current member's check-in history (last 12 months).</summary>
    [HttpGet("my-history")]
    public async Task<ActionResult<IReadOnlyCollection<CheckInDto>>> GetMemberHistory(CancellationToken cancellationToken)
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(memberId, out var id))
            return Unauthorized();

        return Ok(await _service.GetMemberHistoryAsync(id, cancellationToken));
    }
}
