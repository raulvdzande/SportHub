using SportHub.API.Domain.Enums;

namespace SportHub.API.Domain.Entities;

public class MemberSubscription
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid PlanId { get; set; }
    public Guid? NextPlanId { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public MembershipStatus Status { get; set; }
    public bool AutoRenew { get; set; } = true;
    public DateTime? CancelRequestedAtUtc { get; set; }

    public Member Member { get; set; } = null!;
    public MembershipPlan Plan { get; set; } = null!;
    public MembershipPlan? NextPlan { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
