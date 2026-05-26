using System.Net.Http.Headers;
using SportHub.App.Services.Storage;
using SportHub.App.State;

namespace SportHub.App.Services.Auth;

public class ApiAuthorizationMessageHandler : DelegatingHandler
{
    private readonly AppSessionState _sessionState;
    private readonly AppLocalStorage _storage;

    public ApiAuthorizationMessageHandler(AppSessionState sessionState, AppLocalStorage storage)
    {
        _sessionState = sessionState;
        _storage = storage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _sessionState.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            token = await _storage.GetTokenAsync();
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

