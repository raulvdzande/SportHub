using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using SportHub.Shared.DTOs.Auth;
using SportHub.Web.Services.Storage;
using SportHub.Web.State;

namespace SportHub.Web.Services.Auth;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string TokenStorageKey = "sporthub.token";
    private const string UserStorageKey = "sporthub.user";

    private readonly AuthSessionState _sessionState;
    private readonly BrowserStorageService _storage;

    private bool _initialized;

    public ApiAuthenticationStateProvider(AuthSessionState sessionState, BrowserStorageService storage)
    {
        _sessionState = sessionState;
        _storage = storage;
        _sessionState.Changed += NotifyAuthenticationChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_initialized)
        {
            await TryRestoreSessionAsync();
            _initialized = true;
        }

        return BuildAuthenticationState();
    }

    public void NotifyAuthenticationChanged()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthenticationState()));
    }

    private async Task TryRestoreSessionAsync()
    {
        if (_sessionState.IsAuthenticated)
        {
            return;
        }

        string? token;
        string? userJson;
        try
        {
            token = await _storage.GetItemAsync(TokenStorageKey);
            userJson = await _storage.GetItemAsync(UserStorageKey);
        }
        catch
        {
            // If browser storage is unavailable or JS interop fails during startup,
            // fall back to anonymous state instead of breaking app initialization.
            return;
        }

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userJson))
        {
            return;
        }

        StaffUserDto? user;
        try
        {
            user = JsonSerializer.Deserialize<StaffUserDto>(userJson);
        }
        catch
        {
            // Clear invalid persisted session data instead of crashing startup.
            try
            {
                await _storage.RemoveItemAsync(TokenStorageKey);
                await _storage.RemoveItemAsync(UserStorageKey);
            }
            catch
            {
                // Ignore storage cleanup failures; anonymous auth state is still safe.
            }
            return;
        }

        if (user is null)
        {
            return;
        }

        _sessionState.SetSession(token, user);
    }

    private AuthenticationState BuildAuthenticationState()
    {
        if (!_sessionState.IsAuthenticated || _sessionState.User is null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var user = _sessionState.User;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, "Employee")
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        return new AuthenticationState(principal);
    }
}

