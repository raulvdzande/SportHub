using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SportHub.App.State;
using SportHub.Shared.DTOs.Members;

namespace SportHub.App.Services.Api;

public class MemberSubscriptionsApiClient : IMemberSubscriptionsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppSessionState _sessionState;
    private readonly ILogger<MemberSubscriptionsApiClient> _logger;

    private HttpClient ApiClient => _httpClientFactory.CreateClient("Api");

    public MemberSubscriptionsApiClient(
        IHttpClientFactory httpClientFactory,
        AppSessionState sessionState,
        ILogger<MemberSubscriptionsApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _sessionState = sessionState;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<MemberSubscriptionDto>> GetMySubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var memberId = _sessionState.CurrentMember?.Id
            ?? throw new InvalidOperationException("Not logged in — cannot retrieve subscriptions.");

        var client = ApiClient;

        // Use /member/{id} route — avoids the JWT claim-mapping issue in /me/subscriptions
        var response = await client.GetAsync($"api/membersubscriptions/member/{memberId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("GetMySubscriptionsAsync {StatusCode}: {Body}", response.StatusCode, body);
            throw new HttpRequestException(
                $"Failed to retrieve subscriptions ({(int)response.StatusCode}): {body}");
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MemberSubscriptionDto>>(cancellationToken)
               ?? Array.Empty<MemberSubscriptionDto>();
    }

    public async Task<SubscriptionUpgradeQuoteDto?> GetUpgradeQuoteAsync(SubscriptionUpgradeQuoteRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = ApiClient;
        try
        {
            var response = await client.PostAsJsonAsync("api/membersubscriptions/upgrade-quote", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetUpgradeQuoteAsync returned {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SubscriptionUpgradeQuoteDto>(cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "GetUpgradeQuoteAsync timed out");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUpgradeQuoteAsync");
            return null;
        }
    }

    public async Task<MemberSubscriptionDto?> CreateAsync(CreateMemberSubscriptionRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = ApiClient;
        try
        {
            var response = await client.PostAsJsonAsync("api/membersubscriptions", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CreateAsync returned {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "CreateAsync timed out");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateAsync");
            return null;
        }
    }

    public async Task<MemberSubscriptionDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = ApiClient;
        try
        {
            var response = await client.PostAsync($"api/membersubscriptions/{id}/cancel", null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CancelAsync returned {StatusCode} for id {Id}", response.StatusCode, id);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "CancelAsync timed out for id {Id}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CancelAsync for id {Id}", id);
            return null;
        }
    }

    public async Task<MemberSubscriptionDto?> EnableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = ApiClient;
        try
        {
            var response = await client.PostAsync($"api/membersubscriptions/{id}/enable-autorenew", null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("EnableAutoRenewAsync returned {StatusCode} for id {Id}", response.StatusCode, id);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "EnableAutoRenewAsync timed out for id {Id}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EnableAutoRenewAsync for id {Id}", id);
            return null;
        }
    }

    public async Task<MemberSubscriptionDto?> DisableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = ApiClient;
        try
        {
            var response = await client.PostAsync($"api/membersubscriptions/{id}/disable-autorenew", null, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DisableAutoRenewAsync returned {StatusCode} for id {Id}", response.StatusCode, id);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MemberSubscriptionDto>(cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "DisableAutoRenewAsync timed out for id {Id}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DisableAutoRenewAsync for id {Id}", id);
            return null;
        }
    }
}
