using System.Net;
using System.Net.Http.Json;
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

    public ApiAuthService(
        IHttpClientFactory httpClientFactory,
        AppSessionState sessionState,
        AppLocalStorage storage)
    {
        _httpClientFactory = httpClientFactory;
        _sessionState = sessionState;
        _storage = storage;
    }

    public async Task<MemberDto?> LoginAsync(LoginMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiAnonymous");

            var response = await client.PostAsJsonAsync("api/auth/login-member", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var payload = await response.Content.ReadFromJsonAsync<LoginMemberResponseDto>(cancellationToken);

            if (payload is null)
                return null;

            _sessionState.SetSession(payload.AccessToken, null);
            await _storage.SetTokenAsync(payload.AccessToken);

            return await RestoreSessionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LOGIN ERROR: {ex}");
            return null;
        }
    }

    public async Task<MemberDto?> RestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = _sessionState.AccessToken;

            if (string.IsNullOrWhiteSpace(token))
            {
                token = await _storage.GetTokenAsync();

                if (string.IsNullOrWhiteSpace(token))
                    return null;

                _sessionState.SetSession(token, null);
            }

            var client = _httpClientFactory.CreateClient("Api");

            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync("api/members/me", cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP CRASH: {ex}");
                return null;
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized ||
                response.StatusCode is HttpStatusCode.Forbidden)
            {
                await LogoutAsync();
                return null;
            }

            if (!response.IsSuccessStatusCode)
                return null;

            var member = await response.Content.ReadFromJsonAsync<MemberDto>(cancellationToken);

            if (member is null)
                return null;

            _sessionState.SetSession(token, member);
            await _storage.SetMemberAsync(member);

            return member;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RESTORE ERROR: {ex}");
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        _sessionState.ClearSession();
        await _storage.RemoveTokenAsync();
        await _storage.RemoveMemberAsync();
    }

    public async Task<MemberDto> CreateAccountAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ApiAnonymous");
        var response = await client.PostAsJsonAsync("api/members", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MemberDto>(cancellationToken))!;
    }
}