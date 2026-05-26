using System.Net.Http.Json;
using Microsoft.Extensions.Http;
using SportHub.Shared.DTOs.Members;

namespace SportHub.App.Services.Api;

public class MemberSubscriptionsApiClient : IMemberSubscriptionsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MemberSubscriptionsApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyCollection<MemberSubscriptionDto>> GetMySubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/members/me/subscriptions", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<MemberSubscriptionDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MemberSubscriptionDto>>(cancellationToken) ?? Array.Empty<MemberSubscriptionDto>();
    }

    public async Task<SubscriptionUpgradeQuoteDto?> GetUpgradeQuoteAsync(SubscriptionUpgradeQuoteRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/membersubscriptions/upgrade-quote", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<SubscriptionUpgradeQuoteDto>(cancellationToken);
    }

    public async Task<MemberSubscriptionDto?> CreateAsync(CreateMemberSubscriptionRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/membersubscriptions", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
    }

    public async Task<MemberSubscriptionDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsync($"api/membersubscriptions/{id}/cancel", null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
    }

    public async Task<MemberSubscriptionDto?> EnableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsync($"api/membersubscriptions/{id}/enable-autorenew", null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
    }

    public async Task<MemberSubscriptionDto?> DisableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsync($"api/membersubscriptions/{id}/disable-autorenew", null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
    }
}
