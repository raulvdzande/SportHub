namespace SportHub.Shared.DTOs.Payments;

public class CreateStripePaymentIntentResponseDto
{
    public Guid PaymentId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
}

