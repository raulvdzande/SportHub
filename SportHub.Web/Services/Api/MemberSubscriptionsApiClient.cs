using System.Net.Http.Json;
using SportHub.Shared.DTOs.Members;

namespace SportHub.Web.Services.Api;

public interface IMemberSubscriptionsApiClient
{
    Task<MemberSubscriptionDto?> CreateAsync(CreateMemberSubscriptionRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MemberSubscriptionDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto?> EnableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto?> DisableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default);
}

public class MemberSubscriptionsApiClient : IMemberSubscriptionsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MemberSubscriptionsApiClient(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<MemberSubscriptionDto?> CreateAsync(CreateMemberSubscriptionRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/membersubscriptions", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"POST api/membersubscriptions mislukt: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
    }

    public async Task<IReadOnlyCollection<MemberSubscriptionDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"api/membersubscriptions/member/{memberId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"GET api/membersubscriptions/member/{memberId} mislukt: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MemberSubscriptionDto>>(cancellationToken)
               ?? Array.Empty<MemberSubscriptionDto>();
    }

    public async Task<MemberSubscriptionDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsync($"api/membersubscriptions/{id}/cancel", null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"POST api/membersubscriptions/{id}/cancel mislukt: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
    }

    public async Task<MemberSubscriptionDto?> EnableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsync($"api/membersubscriptions/{id}/enable-autorenew", null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"POST api/membersubscriptions/{id}/enable-autorenew mislukt: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
    }

    public async Task<MemberSubscriptionDto?> DisableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsync($"api/membersubscriptions/{id}/disable-autorenew", null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"POST api/membersubscriptions/{id}/disable-autorenew mislukt: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
    }
}