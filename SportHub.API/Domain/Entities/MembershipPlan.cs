using SportHub.API.Domain.Enums;

namespace SportHub.API.Domain.Entities;

public class MembershipPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SubscriptionPeriodType PeriodType { get; set; }
    public int? SessionsPerWeekLimit { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; } = true;

    public ICollection<MemberSubscription> Subscriptions { get; set; } = new List<MemberSubscription>();
}

