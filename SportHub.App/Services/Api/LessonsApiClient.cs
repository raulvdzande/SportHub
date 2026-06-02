using System.Net.Http.Json;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using SportHub.Shared.DTOs.Lessons;

namespace SportHub.App.Services.Api;

public class LessonsApiClient : ILessonsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LessonsApiClient> _logger;

    public LessonsApiClient(IHttpClientFactory httpClientFactory, ILogger<LessonsApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<MobileLessonSummaryDto>> GetMobileScheduleAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var url = $"api/lessons/mobile?fromUtc={Uri.EscapeDataString(fromUtc.ToString("O"))}&toUtc={Uri.EscapeDataString(toUtc.ToString("O"))}";
            var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetMobileScheduleAsync returned {StatusCode} for {Url}", response.StatusCode, url);
                return Array.Empty<MobileLessonSummaryDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MobileLessonSummaryDto>>(cancellationToken);
            return result ?? Array.Empty<MobileLessonSummaryDto>();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "GetMobileScheduleAsync timed out");
            return Array.Empty<MobileLessonSummaryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMobileScheduleAsync");
            return Array.Empty<MobileLessonSummaryDto>();
        }
    }

    public async Task<MobileLessonDetailsDto?> GetMobileDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var response = await client.GetAsync($"api/lessons/{id}/mobile", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetMobileDetailsAsync returned {StatusCode} for id {Id}", response.StatusCode, id);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MobileLessonDetailsDto>(cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "GetMobileDetailsAsync timed out for id {Id}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMobileDetailsAsync for id {Id}", id);
            return null;
        }
    }
}
