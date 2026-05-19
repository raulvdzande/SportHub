namespace SportHub.Shared.DTOs.Members;

public class MemberSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid PlanId { get; set; }
    public Guid? NextPlanId { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool AutoRenew { get; set; }
    public DateTime? CancelRequestedAtUtc { get; set; }
}

