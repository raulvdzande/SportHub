using System.Net.Http.Json;
using Microsoft.Extensions.Http;
using SportHub.Shared.DTOs.Lessons;

namespace SportHub.App.Services.Api;

public class LessonsApiClient : ILessonsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LessonsApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyCollection<MobileLessonSummaryDto>> GetMobileScheduleAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"api/lessons/mobile?fromUtc={Uri.EscapeDataString(fromUtc.ToString("O"))}&toUtc={Uri.EscapeDataString(toUtc.ToString("O"))}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<MobileLessonSummaryDto>();
        }

        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MobileLessonSummaryDto>>(cancellationToken) ?? Array.Empty<MobileLessonSummaryDto>();
    }

    public async Task<MobileLessonDetailsDto?> GetMobileDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"api/lessons/{id}/mobile", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MobileLessonDetailsDto>(cancellationToken);
    }
}
