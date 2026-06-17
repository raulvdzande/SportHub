using SportHub.Shared.DTOs.Notifications;
using System.Text.Json;

namespace SportHub.App.Services.Api;

public class NotificationsApiClient : INotificationsApiClient
{
    private readonly HttpClient _httpClient;

    public NotificationsApiClient(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("Api");
    }

    public async Task<IReadOnlyCollection<NotificationDto>?> GetMyNotificationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/notifications/my", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<NotificationDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    public async Task<bool> MarkReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/notifications/{notificationId}/read", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> MarkAllReadAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync("api/notifications/read-all", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> AcceptWaitlistSpotAsync(Guid notificationId, CancellationToken ct = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[API] AcceptWaitlistSpot called with notificationId={notificationId}");
            var response = await _httpClient.PostAsync($"api/notifications/{notificationId}/accept-waitlist-spot", null, ct);
            System.Diagnostics.Debug.WriteLine($"[API] AcceptWaitlistSpot response: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                System.Diagnostics.Debug.WriteLine($"[API] AcceptWaitlistSpot error: {error}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API] AcceptWaitlistSpot exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeclineWaitlistSpotAsync(Guid notificationId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/notifications/{notificationId}/decline-waitlist-spot", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
