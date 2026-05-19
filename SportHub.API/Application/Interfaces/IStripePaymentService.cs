using SportHub.Shared.DTOs.Payments;

namespace SportHub.API.Application.Interfaces;

public interface IStripePaymentService
{
    Task<IReadOnlyCollection<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CreateStripePaymentIntentResponseDto> CreatePaymentIntentAsync(CreateStripePaymentIntentRequestDto request, CancellationToken cancellationToken = default);
    Task<PaymentDto?> HandleWebhookAsync(string json, string stripeSignature, CancellationToken cancellationToken = default);
    Task<PaymentDto> SimulateSucceededAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    
    Task<CreateCheckoutSessionResponseDto> CreateCheckoutSessionAsync(
        CreateStripePaymentIntentRequestDto request,
        CancellationToken cancellationToken = default);
}
