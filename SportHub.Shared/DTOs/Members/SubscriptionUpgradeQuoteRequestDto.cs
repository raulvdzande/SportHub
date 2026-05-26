using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Members;

public class SubscriptionUpgradeQuoteRequestDto
{
    [Required]
    public Guid CurrentSubscriptionId { get; set; }

    [Required]
    public Guid TargetPlanId { get; set; }

    public DateTime? CalculatedAtUtc { get; set; }
}
