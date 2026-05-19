using System.Net.Http.Json;
using SportHub.Shared.DTOs.Auth;

namespace SportHub.Web.Services.Api;

public interface IAuthMemberApiClient
{
    Task<LoginMemberResponseDto?> LoginAsync(LoginMemberRequestDto request, CancellationToken cancellationToken = default);
}

public class AuthMemberApiClient : IAuthMemberApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthMemberApiClient(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<LoginMemberResponseDto?> LoginAsync(LoginMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ApiAnonymous");
        var response = await client.PostAsJsonAsync("api/auth/login-member", request, cancellationToken);
        return !response.IsSuccessStatusCode ? null : await response.Content.ReadFromJsonAsync<LoginMemberResponseDto>(cancellationToken);
    }
}

