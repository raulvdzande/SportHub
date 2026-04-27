using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using SportHub.Shared.DTOs.Auth;
using SportHub.Web.Services.Storage;
using SportHub.Web.State;
namespace SportHub.Web.Services.Auth;
public class AuthApiService : IAuthApiService
{
    private const string TokenStorageKey = "sporthub.token";
    private const string UserStorageKey = "sporthub.user";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthSessionState _sessionState;
    private readonly BrowserStorageService _storage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    public AuthApiService(
        IHttpClientFactory httpClientFactory,
        AuthSessionState sessionState,
        BrowserStorageService storage,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory = httpClientFactory;
        _sessionState = sessionState;
        _storage = storage;
        _authenticationStateProvider = authenticationStateProvider;
    }
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ApiAnonymous");
        using var response = await client.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        var payload = await response.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken);
        if (payload is null)
        {
            return null;
        }
        _sessionState.SetSession(payload.AccessToken, payload.User);
        await _storage.SetItemAsync(TokenStorageKey, payload.AccessToken);
        await _storage.SetItemAsync(UserStorageKey, JsonSerializer.Serialize(payload.User));
        if (_authenticationStateProvider is ApiAuthenticationStateProvider provider)
        {
            provider.NotifyAuthenticationChanged();
        }
        return payload;
    }
    public async Task LogoutAsync()
    {
        _sessionState.ClearSession();
        await _storage.RemoveItemAsync(TokenStorageKey);
        await _storage.RemoveItemAsync(UserStorageKey);
        if (_authenticationStateProvider is ApiAuthenticationStateProvider provider)
        {
            provider.NotifyAuthenticationChanged();
        }
    }
}
