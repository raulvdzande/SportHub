using SportHub.Shared.DTOs.Members;

namespace SportHub.API.Application.Interfaces;

public interface IMemberService
{
    Task<IReadOnlyCollection<MemberDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MemberDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MemberDto> CreateAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default);
    Task<MemberDto> UpdateAsync(Guid id, UpdateMemberRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
