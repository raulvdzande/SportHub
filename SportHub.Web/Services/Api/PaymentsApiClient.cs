using System.Net.Http.Json;
using SportHub.Shared.DTOs.Payments;

namespace SportHub.Web.Services.Api;

public interface IPaymentsApiClient
{
    Task<CreateStripePaymentIntentResponseDto> CreatePaymentIntentAsync(CreateStripePaymentIntentRequestDto request, CancellationToken cancellationToken = default);
    Task<PaymentDto> SimulateSucceededAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    
    Task<CreateCheckoutSessionResponseDto> CreateCheckoutSessionAsync(
        CreateStripePaymentIntentRequestDto request,
        CancellationToken cancellationToken = default);
}

public class PaymentsApiClient : IPaymentsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentsApiClient(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<CreateStripePaymentIntentResponseDto> CreatePaymentIntentAsync(
        CreateStripePaymentIntentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/stripe/payment-intents", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"POST api/stripe/payment-intents mislukt: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
                null,
                response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<CreateStripePaymentIntentResponseDto>(cancellationToken))!;
    }

    public async Task<PaymentDto> SimulateSucceededAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");

        // The controller expects an object { "paymentIntentId": "..." }, not a bare string
        var dto = new SimulateStripePaymentRequestDto { PaymentIntentId = paymentIntentId };
        var response = await client.PostAsJsonAsync("api/stripe/simulate-succeeded", dto, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"POST api/stripe/simulate-succeeded mislukt: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
                null,
                response.StatusCode);
        }

        return (await response.Content.ReadFromJsonAsync<PaymentDto>(cancellationToken))!;
    }
    
    public async Task<CreateCheckoutSessionResponseDto>
        CreateCheckoutSessionAsync(
            CreateStripePaymentIntentRequestDto request,
            CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var response =
            await client.PostAsJsonAsync(
                "api/stripe/checkout-session",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<CreateCheckoutSessionResponseDto>(
                cancellationToken))!;
    }
}