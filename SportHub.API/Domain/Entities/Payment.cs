using SportHub.API.Domain.Enums;

namespace SportHub.API.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ExternalReference { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }

    public Member Member { get; set; } = null!;
    public MemberSubscription? Subscription { get; set; }
}

