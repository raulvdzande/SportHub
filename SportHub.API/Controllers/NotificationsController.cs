using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.API.Application.Interfaces;
using SportHub.Shared.DTOs.Notifications;
using System.Security.Claims;

namespace SportHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService service, ILogger<NotificationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Get notifications for current member.</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyCollection<NotificationDto>>> GetMine(CancellationToken cancellationToken)
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(memberId, out var id))
            return Unauthorized();

        return Ok(await _service.GetByMemberAsync(id, cancellationToken));
    }

    /// <summary>Mark single notification as read.</summary>
    [HttpPost("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken cancellationToken)
    {
        await _service.MarkReadAsync(notificationId, cancellationToken);
        return NoContent();
    }

    /// <summary>Mark all notifications as read for current member.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(memberId, out var id))
            return Unauthorized();

        await _service.MarkAllReadAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Accept a waitlist spot (from notification).</summary>
    [HttpPost("{notificationId:guid}/accept-waitlist-spot")]
    public async Task<IActionResult> AcceptWaitlistSpot(Guid notificationId, CancellationToken cancellationToken)
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(memberId, out var id))
            return Unauthorized();

        try
        {
            _logger.LogInformation("AcceptWaitlistSpot: notificationId={NotificationId}, memberId={MemberId}", notificationId, id);
            await _service.AcceptWaitlistSpotAsync(notificationId, id, cancellationToken);
            _logger.LogInformation("AcceptWaitlistSpot SUCCESS: notificationId={NotificationId}, memberId={MemberId}", notificationId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("AcceptWaitlistSpot KeyNotFound: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("AcceptWaitlistSpot Unauthorized: {Message}", ex.Message);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("AcceptWaitlistSpot InvalidOperation: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AcceptWaitlistSpot exception: {Message}", ex.Message);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>Decline a waitlist spot (from notification).</summary>
    [HttpPost("{notificationId:guid}/decline-waitlist-spot")]
    public async Task<IActionResult> DeclineWaitlistSpot(Guid notificationId, CancellationToken cancellationToken)
    {
        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(memberId, out var id))
            return Unauthorized();

        await _service.DeclineWaitlistSpotAsync(notificationId, id, cancellationToken);
        return NoContent();
    }
}
