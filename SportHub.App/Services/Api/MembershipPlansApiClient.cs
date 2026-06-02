using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SportHub.Shared.DTOs.Members;

namespace SportHub.App.Services.Api;

public interface IMembershipPlansApiClient
{
    Task<IReadOnlyCollection<MembershipPlanDto>?> GetAllAsync(CancellationToken cancellationToken = default);
}

public class MembershipPlansApiClient : IMembershipPlansApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MembershipPlansApiClient> _logger;

    public MembershipPlansApiClient(IHttpClientFactory httpClientFactory, ILogger<MembershipPlansApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<MembershipPlanDto>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/membershipplans", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("GetAllAsync {StatusCode}: {Body}", response.StatusCode, body);
            throw new HttpRequestException(
                $"Abonnementsplannen ophalen mislukt ({(int)response.StatusCode} {response.StatusCode}): {body}");
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MembershipPlanDto>>(cancellationToken);
    }
}
