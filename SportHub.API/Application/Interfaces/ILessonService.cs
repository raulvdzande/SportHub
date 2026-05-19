using SportHub.Shared.DTOs.Lessons;

namespace SportHub.API.Application.Interfaces;

public interface ILessonService
{
    Task<IReadOnlyCollection<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LessonDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LessonDto> CreateAsync(CreateLessonRequestDto request, CancellationToken cancellationToken = default);
    Task<LessonDto> UpdateAsync(Guid id, UpdateLessonRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LessonRecurrenceRuleDto> CreateRecurrenceRuleAsync(CreateLessonRecurrenceRuleRequestDto request, CancellationToken cancellationToken = default);
    Task<GenerateRecurringLessonsResponseDto> GenerateRecurringLessonsAsync(GenerateRecurringLessonsRequestDto request, CancellationToken cancellationToken = default);
}
