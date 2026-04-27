using System.Net.Http.Headers;
using SportHub.Web.State;

namespace SportHub.Web.Services.Auth;

public class ApiAuthorizationMessageHandler : DelegatingHandler
{
    private readonly AuthSessionState _sessionState;

    public ApiAuthorizationMessageHandler(AuthSessionState sessionState)
    {
        _sessionState = sessionState;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_sessionState.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessionState.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
