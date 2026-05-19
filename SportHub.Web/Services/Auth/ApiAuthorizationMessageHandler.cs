using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;
using SportHub.Web.Services.Storage;
using SportHub.Web.State;

namespace SportHub.Web.Services.Auth;

public class ApiAuthorizationMessageHandler : DelegatingHandler
{
    private const string TokenStorageKey = "sporthub.token";
    private const string UserStorageKey = "sporthub.user";

    private readonly AuthSessionState _sessionState;
    private readonly BrowserStorageService _storage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public ApiAuthorizationMessageHandler(
        AuthSessionState sessionState,
        BrowserStorageService storage,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _sessionState = sessionState;
        _storage = storage;
        _authenticationStateProvider = authenticationStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = _sessionState.AccessToken;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            accessToken = await _storage.GetItemAsync(TokenStorageKey);
        }

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && _sessionState.IsAuthenticated)
        {
            _ = Task.Run(async () =>
            {
                _sessionState.ClearSession();
                await _storage.RemoveItemAsync(TokenStorageKey);
                await _storage.RemoveItemAsync(UserStorageKey);
                if (_authenticationStateProvider is ApiAuthenticationStateProvider provider)
                {
                    provider.NotifyAuthenticationChanged();
                }
            }, CancellationToken.None);
        }

        return response;
    }
}