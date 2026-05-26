using System.ComponentModel.DataAnnotations;

namespace SportHub.API.Controllers.Requests;

public class CreateSubscriptionUpgradeQuoteRequest
{
    [Required]
    public Guid CurrentSubscriptionId { get; set; }

    [Required]
    public Guid TargetPlanId { get; set; }

    public DateTime? CalculatedAtUtc { get; set; }
}
