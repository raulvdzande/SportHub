using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SportHub.Shared.DTOs.Payments;

namespace SportHub.App.Services.Api;

public interface IStripeApiClient
{
    Task<CreateCheckoutSessionResponseDto?> CreateCheckoutSessionAsync(
        CreateStripePaymentIntentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<CreateStripePaymentIntentResponseDto?> CreatePaymentIntentAsync(
        CreateStripePaymentIntentRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> SimulateSucceededAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);
}

public class StripeApiClient : IStripeApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<StripeApiClient> _logger;

    public StripeApiClient(IHttpClientFactory httpClientFactory, ILogger<StripeApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<CreateCheckoutSessionResponseDto?> CreateCheckoutSessionAsync(
        CreateStripePaymentIntentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var response = await client.PostAsJsonAsync("api/stripe/checkout-session", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("CreateCheckoutSessionAsync {StatusCode}: {Body}", response.StatusCode, body);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<CreateCheckoutSessionResponseDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCheckoutSessionAsync failed");
            return null;
        }
    }

    public async Task<CreateStripePaymentIntentResponseDto?> CreatePaymentIntentAsync(
        CreateStripePaymentIntentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var response = await client.PostAsJsonAsync("api/stripe/payment-intents", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("CreatePaymentIntentAsync {StatusCode}: {Body}", response.StatusCode, body);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<CreateStripePaymentIntentResponseDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePaymentIntentAsync failed");
            return null;
        }
    }

    public async Task<bool> SimulateSucceededAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var response = await client.PostAsJsonAsync(
                "api/stripe/simulate-succeeded",
                new SimulateStripePaymentRequestDto { PaymentIntentId = paymentIntentId },
                cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SimulateSucceededAsync failed");
            return false;
        }
    }
}
