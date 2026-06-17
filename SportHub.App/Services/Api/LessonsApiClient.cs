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

    public async Task<IReadOnlyCollection<MobileLessonSummaryDto>> GetMobileScheduleAsync(DateTime fromUtc, DateTime toUtc, Guid? instructorId = null, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var url = $"api/lessons/mobile?fromUtc={Uri.EscapeDataString(fromUtc.ToString("O"))}&toUtc={Uri.EscapeDataString(toUtc.ToString("O"))}";
            if (instructorId.HasValue)
                url += $"&instructorId={instructorId.Value}";
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

    public async Task<MobileLessonSummaryDto?> CreateAsync(CreateLessonRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var response = await client.PostAsJsonAsync("api/lessons", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CreateAsync returned {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"Failed to create lesson: {response.StatusCode}", null, response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<MobileLessonSummaryDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateAsync");
            throw;
        }
    }

    public async Task<MobileLessonSummaryDto?> UpdateAsync(Guid id, UpdateLessonRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var response = await client.PutAsJsonAsync($"api/lessons/{id}", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("UpdateAsync returned {StatusCode} for id {Id}", response.StatusCode, id);
                throw new HttpRequestException($"Failed to update lesson: {response.StatusCode}", null, response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<MobileLessonSummaryDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateAsync for id {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        try
        {
            var response = await client.DeleteAsync($"api/lessons/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DeleteAsync returned {StatusCode} for id {Id}", response.StatusCode, id);
                throw new HttpRequestException($"Failed to delete lesson: {response.StatusCode}", null, response.StatusCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteAsync for id {Id}", id);
            throw;
        }
    }
}
