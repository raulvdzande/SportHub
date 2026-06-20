using SportHub.Shared.DTOs.Members;

namespace SportHub.App.ViewModels;

public class SubscriptionDisplayItem
{
    public MemberSubscriptionDto Subscription { get; }
    public MembershipPlanDto?    Plan         { get; }

    public SubscriptionDisplayItem(MemberSubscriptionDto subscription, MembershipPlanDto? plan)
    {
        Subscription = subscription;
        Plan         = plan;
    }

    public Guid   Id       => Subscription.Id;
    public string PlanName => Plan?.Name ?? "Onbekend plan";

    public string PeriodLabel => Plan?.PeriodType switch
    {
        "Monthly" => "Maandelijks",
        "Yearly"  => "Jaarlijks",
        _         => Plan?.PeriodType ?? string.Empty
    };

    public string SessionsLabel => Plan?.SessionsPerWeekLimit.HasValue == true
        ? $"{Plan.SessionsPerWeekLimit}x per week"
        : "Onbeperkt";

    public decimal Price => Plan?.Price ?? 0;

    public string StartDate => Subscription.StartsAtUtc.ToLocalTime().ToString("dd/MM/yyyy");
    public string EndDate   => Subscription.EndsAtUtc.ToLocalTime().ToString("dd/MM/yyyy");
    public string Status    => Subscription.Status;

    public bool IsActive          => Subscription.Status == "Active";
    public bool AutoRenew         => Subscription.AutoRenew;
    public bool CanDisableAutoRenew => IsActive && Subscription.AutoRenew;
    public bool CanEnableAutoRenew  => IsActive && !Subscription.AutoRenew;

    public string AutoRenewLabel => Subscription.AutoRenew
        ? "Auto-renewal: On"
        : "Auto-renewal: Off (expires on the end date)";
}
