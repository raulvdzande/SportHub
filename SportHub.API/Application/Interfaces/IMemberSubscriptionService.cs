using SportHub.Shared.DTOs.Members;

namespace SportHub.API.Application.Interfaces;

public interface IMemberSubscriptionService
{
    Task<IReadOnlyCollection<MemberSubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MemberSubscriptionDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto> CreateAsync(CreateMemberSubscriptionRequestDto request, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto> EnableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto> DisableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default);
}
