using System.Net.Http.Json;
using SportHub.Shared.DTOs.Lessons;
using SportHub.Shared.DTOs.Locations;

namespace SportHub.Web.Services.Api;

public interface ILessonsApiClient
{
    Task<IEnumerable<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LessonDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LessonDto> CreateAsync(CreateLessonRequestDto request, CancellationToken cancellationToken = default);
    Task<LessonDto> UpdateAsync(Guid id, UpdateLessonRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<LessonDto>> GetByRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<LessonRecurrenceRuleDto> CreateRecurrenceRuleAsync(CreateLessonRecurrenceRuleRequestDto request, CancellationToken cancellationToken = default);
    Task<GenerateRecurringLessonsResponseDto> GenerateRecurringAsync(GenerateRecurringLessonsRequestDto request, CancellationToken cancellationToken = default);
}

