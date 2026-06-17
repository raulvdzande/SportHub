using SportHub.Shared.DTOs.Notifications;

namespace SportHub.API.Application.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyCollection<NotificationDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task CreateAsync(Guid memberId, string type, string title, string message, Guid? lessonId = null, Guid? subscriptionId = null, CancellationToken cancellationToken = default);
    Task AcceptWaitlistSpotAsync(Guid notificationId, Guid memberId, CancellationToken cancellationToken = default);
    Task DeclineWaitlistSpotAsync(Guid notificationId, Guid memberId, CancellationToken cancellationToken = default);
}
