using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportHub.Shared.DTOs.Notifications;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext dbContext, ILogger<NotificationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<NotificationDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FETCHING NOTIFICATIONS for member {MemberId}", memberId);

        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.MemberId == memberId && x.Status == NotificationStatus.Pending)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("FOUND {Count} notifications for member {MemberId}", notifications.Count, memberId);
        foreach (var n in notifications)
        {
            _logger.LogInformation("  - Notification: {NotificationId}, Type={Type}, Status={Status}", n.Id, n.Type, n.Status);
        }

        return notifications.Select(MapToDto).ToList();
    }

    public async Task MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Marking notification {NotificationId} as read", notificationId);

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken)
            ?? throw new KeyNotFoundException("Notification not found.");

        notification.ReadAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Marking all notifications as read for member {MemberId}", memberId);

        var unreadNotifications = await _dbContext.Notifications
            .Where(x => x.MemberId == memberId && x.ReadAtUtc == null)
            .ToListAsync(cancellationToken);

        if (unreadNotifications.Count == 0)
        {
            _logger.LogInformation("No unread notifications found for member {MemberId}", memberId);
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.ReadAtUtc = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Marked {Count} notifications as read for member {MemberId}", unreadNotifications.Count, memberId);
    }

    public async Task CreateAsync(Guid memberId, string type, string title, string message, Guid? lessonId = null, Guid? subscriptionId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating notification for member {MemberId} with type {Type}", memberId, type);

        if (!Enum.TryParse<NotificationType>(type, ignoreCase: true, out var notificationType))
        {
            throw new InvalidOperationException($"Invalid notification type: {type}. Valid types are: {string.Join(", ", Enum.GetNames(typeof(NotificationType)))}");
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            LessonId = lessonId,
            SubscriptionId = subscriptionId,
            Type = notificationType,
            Status = NotificationStatus.Pending,
            Title = title.Trim(),
            Message = message.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            SentAtUtc = null,
            ReadAtUtc = null
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification {NotificationId} created successfully for member {MemberId}", notification.Id, memberId);
    }

    public async Task AcceptWaitlistSpotAsync(Guid notificationId, Guid memberId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 AcceptWaitlistSpot START: notificationId={NotificationId}, memberId={MemberId}", notificationId, memberId);

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken)
            ?? throw new KeyNotFoundException($"❌ Notification not found: {notificationId}");

        _logger.LogInformation("✓ Notification found: Type={Type}, LessonId={LessonId}, MemberId={NotifMemberId}, Status={Status}",
            notification.Type, notification.LessonId, notification.MemberId, notification.Status);

        if (notification.MemberId != memberId)
            throw new UnauthorizedAccessException($"❌ MemberId mismatch: notification.MemberId={notification.MemberId} vs memberId={memberId}");

        if (notification.Type != NotificationType.WaitlistSpotOpened || notification.LessonId == null)
            throw new InvalidOperationException($"❌ Invalid notification type: Type={notification.Type}, LessonId={notification.LessonId}");

        _logger.LogInformation("🔍 Looking for waitlist reservation: LessonId={LessonId}, MemberId={MemberId}", notification.LessonId, memberId);

        var reservation = await _dbContext.LessonReservations
            .FirstOrDefaultAsync(
                x => x.LessonId == notification.LessonId &&
                     x.MemberId == memberId &&
                     x.Status == LessonReservationStatus.Waitlisted,
                cancellationToken)
            ?? throw new KeyNotFoundException($"❌ Waitlist reservation not found for lesson {notification.LessonId}, member {memberId}");

        _logger.LogInformation("✓ Reservation found: Id={ReservationId}, Status={Status}, WaitlistPosition={Position}",
            reservation.Id, reservation.Status, reservation.WaitlistPosition);

        _logger.LogInformation("🔄 Promoting to Reserved...");
        reservation.Status = LessonReservationStatus.Reserved;
        reservation.WaitlistPosition = null;

        notification.Status = NotificationStatus.Sent;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("✓ Database saved");

        // After saving, update remaining waitlist positions
        var remainingWaitlisted = await _dbContext.LessonReservations
            .Where(x => x.LessonId == notification.LessonId &&
                       x.Status == LessonReservationStatus.Waitlisted)
            .OrderBy(x => x.WaitlistPosition)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("🔍 Remaining waitlisted: {Count}", remainingWaitlisted.Count);

        for (int i = 0; i < remainingWaitlisted.Count; i++)
        {
            remainingWaitlisted[i].WaitlistPosition = i + 1;
        }

        if (remainingWaitlisted.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("✓ Waitlist positions updated");
        }

        _logger.LogInformation("✅ AcceptWaitlistSpot SUCCESS: Member {MemberId} promoted from waitlist for lesson {LessonId}", memberId, notification.LessonId);
    }

    public async Task DeclineWaitlistSpotAsync(Guid notificationId, Guid memberId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Member {MemberId} declined waitlist spot from notification {NotificationId}", memberId, notificationId);

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.MemberId != memberId)
            throw new UnauthorizedAccessException("Cannot decline someone else's notification.");

        if (notification.Type != NotificationType.WaitlistSpotOpened || notification.LessonId == null)
            throw new InvalidOperationException("This notification is not a waitlist spot offer.");

        notification.Status = NotificationStatus.Sent;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var nextWaitlisted = await _dbContext.LessonReservations
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Workout)
            .Where(x => x.LessonId == notification.LessonId &&
                       x.Status == LessonReservationStatus.Waitlisted)
            .OrderBy(x => x.WaitlistPosition)
            .FirstOrDefaultAsync(cancellationToken);

        if (nextWaitlisted != null)
        {
            var nextNotification = new Notification
            {
                Id = Guid.NewGuid(),
                MemberId = nextWaitlisted.MemberId,
                LessonId = notification.LessonId,
                Title = "Spot Available",
                Message = $"A spot opened for {nextWaitlisted.Lesson.Workout.Name} at {nextWaitlisted.Lesson.StartTimeUtc:HH:mm}. Accept to join?",
                Type = NotificationType.WaitlistSpotOpened,
                Status = NotificationStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Notifications.Add(nextNotification);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sent waitlist spot to next member {MemberId} for lesson {LessonId}", nextWaitlisted.MemberId, notification.LessonId);
        }
    }

    private static NotificationDto MapToDto(Notification notification) => new()
    {
        Id = notification.Id,
        Type = notification.Type.ToString(),
        Status = notification.Status.ToString(),
        Title = notification.Title,
        Message = notification.Message,
        CreatedAtUtc = notification.CreatedAtUtc,
        ReadAtUtc = notification.ReadAtUtc,
        IsRead = notification.ReadAtUtc != null,
        LessonId = notification.LessonId,
        SubscriptionId = notification.SubscriptionId
    };
}
