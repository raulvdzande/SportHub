using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Members;

public class CreateMemberSubscriptionRequestDto
{
    [Required]
    public Guid MemberId { get; set; }

    [Required]
    public Guid PlanId { get; set; }

    public Guid? NextPlanId { get; set; }

    public DateTime? StartsAtUtc { get; set; }

    public bool AutoRenew { get; set; } = true;
}

