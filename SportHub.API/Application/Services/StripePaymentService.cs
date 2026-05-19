using Stripe;
using SportHub.API.Application.Interfaces;
using SportHub.Shared.DTOs.Payments;
using Stripe.Checkout;

namespace SportHub.API.Application.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        IConfiguration configuration,
        ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        StripeConfiguration.ApiKey =
            _configuration["Stripe:SecretKey"];
    }
    
    public async Task<CreateCheckoutSessionResponseDto>
        CreateCheckoutSessionAsync(
            CreateStripePaymentIntentRequestDto request,
            CancellationToken cancellationToken = default)
    {
        var options = new SessionCreateOptions
        {
            Metadata = new Dictionary<string, string>
            {
                { "MemberId", request.MemberId.ToString() },
                { "PlanId", request.SubscriptionId?.ToString() ?? "" }
            },
            
            Mode = "payment",

            SuccessUrl = $"http://localhost:5003/checkout-success?planId={request.SubscriptionId}",
            CancelUrl = "http://localhost:5003/checkout-cancel",

            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,

                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.Currency,

                        UnitAmount =
                            (long)(request.Amount * 100),

                        ProductData =
                            new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "SportHub Membership"
                            }
                    }
                }
            }
        };

        var service = new SessionService();

        var session =
            await service.CreateAsync(
                options,
                cancellationToken: cancellationToken);

        return new CreateCheckoutSessionResponseDto
        {
            SessionId = session.Id,
            CheckoutUrl = session.Url
        };
    }

    public async Task<CreateStripePaymentIntentResponseDto>
        CreatePaymentIntentAsync(
            CreateStripePaymentIntentRequestDto request,
            CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = request.Currency.ToLower(),
                AutomaticPaymentMethods =
                    new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    }
            };

            var service = new PaymentIntentService();

            var paymentIntent =
                await service.CreateAsync(options, null, cancellationToken);

            return new CreateStripePaymentIntentResponseDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                PublishableKey =
                    _configuration["Stripe:PublishableKey"] ?? "",
                PaymentId = Guid.NewGuid()
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex,
                "Stripe error while creating PaymentIntent");

            throw new Exception(
                $"Stripe fout: {ex.StripeError?.Message}");
        }
    }

    public async Task<PaymentDto> SimulateSucceededAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(500, cancellationToken);

        return new PaymentDto
        {
            Id = Guid.NewGuid(),
            PaymentIntentId = paymentIntentId,
            Status = "Succeeded",
            Amount = 0,
            Currency = "eur",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public async Task<IReadOnlyCollection<PaymentDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new List<PaymentDto>());
    }

    public async Task<PaymentDto?> HandleWebhookAsync(
        string json,
        string stripeSignature,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return null;
    }
}