using SportHub.Shared.DTOs.Notifications;

namespace SportHub.App.Services.Api;

public interface INotificationsApiClient
{
    Task<IReadOnlyCollection<NotificationDto>?> GetMyNotificationsAsync(CancellationToken ct = default);
    Task<bool> MarkReadAsync(Guid notificationId, CancellationToken ct = default);
    Task<bool> MarkAllReadAsync(CancellationToken ct = default);
    Task<bool> AcceptWaitlistSpotAsync(Guid notificationId, CancellationToken ct = default);
    Task<bool> DeclineWaitlistSpotAsync(Guid notificationId, CancellationToken ct = default);
}
