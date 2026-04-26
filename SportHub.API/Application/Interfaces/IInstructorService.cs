using SportHub.Shared.DTOs.Instructors;

namespace SportHub.API.Application.Interfaces;

public interface IInstructorService
{
    Task<IReadOnlyCollection<InstructorDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InstructorDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InstructorDto> CreateAsync(CreateInstructorRequestDto request, CancellationToken cancellationToken = default);
    Task<InstructorDto> UpdateAsync(Guid id, UpdateInstructorRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
