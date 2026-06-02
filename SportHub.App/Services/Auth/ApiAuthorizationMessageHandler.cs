using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using SportHub.App.Services.Storage;
using SportHub.App.State;

namespace SportHub.App.Services.Auth;

public class ApiAuthorizationMessageHandler : DelegatingHandler
{
    private readonly AppSessionState _sessionState;
    private readonly AppLocalStorage _storage;
    private readonly ILogger<ApiAuthorizationMessageHandler> _logger;

    public ApiAuthorizationMessageHandler(AppSessionState sessionState, AppLocalStorage storage, ILogger<ApiAuthorizationMessageHandler> logger)
    {
        _sessionState = sessionState;
        _storage = storage;
        _logger = logger;
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
        else
        {
            _logger.LogDebug("No token found for request {Method} {Uri}", request.Method, request.RequestUri);
        }

        try
        {
            var resp = await base.SendAsync(request, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("API responded {StatusCode} for {Method} {Uri}", resp.StatusCode, request.Method, request.RequestUri);
            }

            return resp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending request to API {Method} {Uri}", request.Method, request.RequestUri);
            throw;
        }
    }
}
