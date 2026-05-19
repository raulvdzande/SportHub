using System.Net.Http.Json;
using SportHub.Shared.DTOs.Members;

namespace SportHub.Web.Services.Api;

public interface IMembersApiClient
{
    Task<IEnumerable<MemberDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MemberDto> CreateAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default);
}

public class MembersApiClient : IMembersApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MembersApiClient(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IEnumerable<MemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/members", cancellationToken);
        return !response.IsSuccessStatusCode ? Enumerable.Empty<MemberDto>() : await response.Content.ReadFromJsonAsync<IEnumerable<MemberDto>>(cancellationToken) ?? Enumerable.Empty<MemberDto>();
    }

    public async Task<MemberDto> CreateAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ApiAnonymous");
        var response = await client.PostAsJsonAsync("api/members", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MemberDto>(cancellationToken))!;
    }
}

