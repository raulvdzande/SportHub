using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Payments;

public class CreateStripePaymentIntentRequestDto
{
    [Required]
    public Guid MemberId { get; set; }

    public Guid? SubscriptionId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "eur";
}