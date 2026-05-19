using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using SportHub.Shared.DTOs.Members;

namespace SportHub.Web.Services.Api;

public interface IMembershipPlansApiClient
{
    Task<IEnumerable<MembershipPlanDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MembershipPlanDto?> CreateAsync(MembershipPlanDto plan, CancellationToken cancellationToken = default);
}

public class MembershipPlansApiClient : IMembershipPlansApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MembershipPlansApiClient(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IEnumerable<MembershipPlanDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/membershipplans", cancellationToken);
        return !response.IsSuccessStatusCode
            ? Enumerable.Empty<MembershipPlanDto>()
            : await response.Content.ReadFromJsonAsync<IEnumerable<MembershipPlanDto>>(cancellationToken) ?? Enumerable.Empty<MembershipPlanDto>();
    }

    public async Task<MembershipPlanDto?> CreateAsync(MembershipPlanDto plan, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/membershipplans", plan, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MembershipPlanDto>(cancellationToken);
    }
}
