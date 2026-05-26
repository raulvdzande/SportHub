namespace SportHub.Shared.DTOs.Members;

public class SubscriptionUpgradeQuoteDto
{
    public Guid CurrentSubscriptionId { get; set; }
    public Guid CurrentPlanId { get; set; }
    public string CurrentPlanName { get; set; } = string.Empty;
    public decimal CurrentPlanPrice { get; set; }
    public DateTime CurrentSubscriptionEndsAtUtc { get; set; }
    public Guid TargetPlanId { get; set; }
    public string TargetPlanName { get; set; } = string.Empty;
    public decimal TargetPlanPrice { get; set; }
    public decimal RemainingCredit { get; set; }
    public decimal AmountToPay { get; set; }
    public string Currency { get; set; } = string.Empty;
    public double RemainingRatio { get; set; }
    public int RemainingDays { get; set; }
    public DateTime CalculatedAtUtc { get; set; }
}
