using System.Net.Http.Json;
using Microsoft.Extensions.Http;
using SportHub.App.Services.Storage;
using SportHub.App.State;
using SportHub.Shared.DTOs.Auth;
using SportHub.Shared.DTOs.Members;

namespace SportHub.App.Services.Auth;

public class ApiAuthService : IAuthApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppSessionState _sessionState;
    private readonly AppLocalStorage _storage;

    public ApiAuthService(IHttpClientFactory httpClientFactory, AppSessionState sessionState, AppLocalStorage storage)
    {
        _httpClientFactory = httpClientFactory;
        _sessionState = sessionState;
        _storage = storage;
    }

    public async Task<MemberDto?> LoginAsync(LoginMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ApiAnonymous");
        var response = await client.PostAsJsonAsync("api/auth/login-member", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<LoginMemberResponseDto>(cancellationToken);
        if (payload is null)
        {
            return null;
        }

        _sessionState.SetSession(payload.AccessToken, null);
        await _storage.SetTokenAsync(payload.AccessToken);

        var member = await RestoreSessionAsync(cancellationToken);
        return member;
    }

    public async Task<MemberDto?> RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var token = _sessionState.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            token = await _storage.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            _sessionState.SetSession(token, _sessionState.CurrentMember);
        }

        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/members/me", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var member = await response.Content.ReadFromJsonAsync<MemberDto>(cancellationToken);
        if (member is null)
        {
            return null;
        }

        _sessionState.SetSession(token, member);
        await _storage.SetMemberAsync(member);
        return member;
    }

    public async Task LogoutAsync()
    {
        _sessionState.ClearSession();
        await _storage.RemoveTokenAsync();
        await _storage.RemoveMemberAsync();
    }
}
