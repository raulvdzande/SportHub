using SportHub.Shared.DTOs.Members;

namespace SportHub.App.Services.Api;

public interface IMemberSubscriptionsApiClient
{
    Task<IReadOnlyCollection<MemberSubscriptionDto>> GetMySubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionUpgradeQuoteDto?> GetUpgradeQuoteAsync(SubscriptionUpgradeQuoteRequestDto request, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto?> CreateAsync(CreateMemberSubscriptionRequestDto request, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto?> EnableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MemberSubscriptionDto?> DisableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default);
}

