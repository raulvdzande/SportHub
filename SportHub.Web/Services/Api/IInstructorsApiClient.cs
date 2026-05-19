using Microsoft.AspNetCore.Components.Forms;
using SportHub.Shared.DTOs.Instructors;
namespace SportHub.Web.Services.Api;
public interface IInstructorsApiClient
{
    Task<IReadOnlyCollection<InstructorDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InstructorDto> CreateAsync(CreateInstructorRequestDto request, IBrowserFile? photo = null, CancellationToken cancellationToken = default);
    Task<InstructorDto> UpdateAsync(Guid id, UpdateInstructorRequestDto request, IBrowserFile? photo = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
