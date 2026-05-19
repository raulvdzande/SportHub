namespace SportHub.Shared.DTOs.Payments;

public class PaymentDto
{
    public Guid Id { get; set; }
    public string? PaymentIntentId { get; set; }
    public Guid MemberId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

