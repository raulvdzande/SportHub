using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Payments;

public class SimulateStripePaymentRequestDto
{
    [Required]
    public string PaymentIntentId { get; set; } = string.Empty;
}

