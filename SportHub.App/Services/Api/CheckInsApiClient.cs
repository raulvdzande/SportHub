using SportHub.Shared.DTOs.CheckIns;
using System.Text.Json;

namespace SportHub.App.Services.Api;

public class CheckInsApiClient : ICheckInsApiClient
{
    private readonly HttpClient _httpClient;

    public CheckInsApiClient(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("Api");
    }

    public async Task<CheckInDto?> CreateAsync(CreateCheckInRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/checkins", content, ct);
            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<CheckInDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    public async Task<IReadOnlyCollection<CheckInDto>?> GetByLessonAsync(Guid lessonId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/checkins/lesson/{lessonId}", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<CheckInDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    public async Task<IReadOnlyCollection<CheckInDto>?> GetMemberHistoryAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/checkins/my-history", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<CheckInDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }
}
