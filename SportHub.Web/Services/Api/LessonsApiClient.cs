using System.Net.Http.Json;
using SportHub.Shared.DTOs.Lessons;

namespace SportHub.Web.Services.Api;

public class LessonsApiClient : ILessonsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LessonsApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/lessons", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Enumerable.Empty<LessonDto>();
        }

        return await response.Content.ReadFromJsonAsync<IEnumerable<LessonDto>>(cancellationToken) ?? Enumerable.Empty<LessonDto>();
    }

    public async Task<LessonDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"api/lessons/{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LessonDto>(cancellationToken);
    }

    public async Task<LessonDto> CreateAsync(CreateLessonRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/lessons", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LessonDto>(cancellationToken))!;
    }

    public async Task<LessonDto> UpdateAsync(Guid id, UpdateLessonRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PutAsJsonAsync($"api/lessons/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LessonDto>(cancellationToken))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.DeleteAsync($"api/lessons/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<LessonDto>> GetByRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Where(x => x.StartTimeUtc >= fromUtc && x.StartTimeUtc <= toUtc);
    }

    public async Task<LessonRecurrenceRuleDto> CreateRecurrenceRuleAsync(CreateLessonRecurrenceRuleRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/lessons/recurrence-rules", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LessonRecurrenceRuleDto>(cancellationToken))!;
    }

    public async Task<GenerateRecurringLessonsResponseDto> GenerateRecurringAsync(GenerateRecurringLessonsRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/lessons/generate-recurring", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GenerateRecurringLessonsResponseDto>(cancellationToken))!;
    }
}
