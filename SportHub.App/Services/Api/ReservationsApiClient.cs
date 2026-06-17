using SportHub.Shared.DTOs.Reservations;
using System.Text.Json;

namespace SportHub.App.Services.Api;

public class ReservationsApiClient : IReservationsApiClient
{
    private readonly HttpClient _httpClient;

    public ReservationsApiClient(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("Api");
    }

    public async Task<IReadOnlyCollection<LessonReservationDto>?> GetMyReservationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/reservations/my", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<LessonReservationDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    public async Task<LessonReservationDto?> CreateAsync(CreateReservationRequestDto request, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/reservations", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"409:{error}", null, response.StatusCode);
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<LessonReservationDto>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<LessonReservationDto?> CancelAsync(Guid reservationId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/reservations/{reservationId}", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<LessonReservationDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    public async Task<Guid?> RemoveParticipantAsync(Guid lessonId, Guid memberId, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"api/reservations/lesson/{lessonId}/participant/{memberId}", ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Error removing participant: {error}", null, response.StatusCode);
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrEmpty(json) || json == "null")
            return null;

        return JsonSerializer.Deserialize<Guid?>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<IReadOnlyCollection<LessonReservationDto>?> GetByLessonAsync(Guid lessonId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/reservations/lesson/{lessonId}", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<LessonReservationDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    public async Task<IReadOnlyCollection<int>?> GetTakenBikesAsync(Guid lessonId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/reservations/lesson/{lessonId}/taken-bikes", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<int>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }
}
