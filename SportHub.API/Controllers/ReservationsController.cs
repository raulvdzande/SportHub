using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.API.Application.Interfaces;
using SportHub.Shared.DTOs.Reservations;
using System.Security.Claims;

namespace SportHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly ILessonReservationService _service;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(ILessonReservationService service, ILogger<ReservationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Get current member's reservations.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyCollection<LessonReservationDto>>> GetMyReservations(CancellationToken cancellationToken)
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(memberId, out var id))
            return Unauthorized();

        return Ok(await _service.GetByMemberAsync(id, cancellationToken));
    }

    /// <summary>Get reservations for a lesson (instructors + admins).</summary>
    [HttpGet("lesson/{lessonId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<LessonReservationDto>>> GetByLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByLessonAsync(lessonId, cancellationToken));
    }

    /// <summary>Create reservation (max 7 days advance, respects subscription limits).</summary>
    [HttpPost]
    public async Task<ActionResult<LessonReservationDto>> Create(CreateReservationRequestDto request, CancellationToken cancellationToken)
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(memberId, out var id))
            return Unauthorized();

        var result = await _service.CreateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Cancel reservation (only up to 1 hour before lesson).</summary>
    [HttpDelete("{reservationId:guid}")]
    public async Task<ActionResult<LessonReservationDto>> Cancel(Guid reservationId, CancellationToken cancellationToken)
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(memberId, out var id))
            return Unauthorized();

        var result = await _service.CancelAsync(reservationId, id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get taken bike numbers for a spinning lesson.</summary>
    [HttpGet("lesson/{lessonId:guid}/taken-bikes")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<int>>> GetTakenBikes(Guid lessonId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetTakenBikesAsync(lessonId, cancellationToken));
    }

    /// <summary>Remove a participant from a lesson (instructor only). Promotes waitlisted member and sends notification.</summary>
    [HttpDelete("lesson/{lessonId:guid}/participant/{memberId:guid}")]
    public async Task<ActionResult<Guid?>> RemoveParticipant(Guid lessonId, Guid memberId, CancellationToken cancellationToken)
    {
        var notificationId = await _service.RemoveParticipantAsync(lessonId, memberId, cancellationToken);
        return Ok(notificationId);
    }
}
