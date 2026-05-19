using SportHub.Shared.DTOs.Members;

namespace SportHub.API.Application.Interfaces;

public interface IMembershipPlanService
{
    Task<IReadOnlyCollection<MembershipPlanDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MembershipPlanDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanRequestDto request, CancellationToken cancellationToken = default);
    Task<MembershipPlanDto> UpdateAsync(Guid id, UpdateMembershipPlanRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
