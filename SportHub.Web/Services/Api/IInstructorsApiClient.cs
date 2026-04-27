using Microsoft.AspNetCore.Components.Forms;
using SportHub.Shared.DTOs.Instructors;
namespace SportHub.Web.Services.Api;
public interface IInstructorsApiClient
{
    Task<IReadOnlyCollection<InstructorDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InstructorDto> CreateAsync(string fullName, IBrowserFile? photo, bool isTbd, bool isActive, CancellationToken cancellationToken = default);
}
