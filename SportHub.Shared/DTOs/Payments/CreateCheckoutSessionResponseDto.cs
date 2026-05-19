namespace SportHub.Shared.DTOs.Payments;

public class CreateCheckoutSessionResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
}