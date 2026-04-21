using SportHub.API.Domain.Enums;

namespace SportHub.API.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid? LessonId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SentAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }

    public Member Member { get; set; } = null!;
    public Lesson? Lesson { get; set; }
    public MemberSubscription? Subscription { get; set; }
}

