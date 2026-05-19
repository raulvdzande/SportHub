using Microsoft.EntityFrameworkCore;
using SportHub.Shared.DTOs.Members;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class MemberSubscriptionService : IMemberSubscriptionService
{
    private readonly AppDbContext _dbContext;

    public MemberSubscriptionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<MemberSubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemberSubscriptions
            .AsNoTracking()
            .OrderByDescending(x => x.StartsAtUtc)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<MemberSubscriptionDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemberSubscriptions
            .AsNoTracking()
            .Where(x => x.MemberId == memberId)
            .OrderByDescending(x => x.StartsAtUtc)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<MemberSubscriptionDto> CreateAsync(CreateMemberSubscriptionRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Members.AnyAsync(x => x.Id == request.MemberId, cancellationToken))
        {
            throw new KeyNotFoundException("Member not found.");
        }

        var plan = await _dbContext.MembershipPlans
            .FirstOrDefaultAsync(x => x.Id == request.PlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Membership plan not found.");

        if (request.NextPlanId.HasValue && !await _dbContext.MembershipPlans.AnyAsync(x => x.Id == request.NextPlanId.Value, cancellationToken))
        {
            throw new KeyNotFoundException("Next plan not found.");
        }

        var starts = request.StartsAtUtc ?? DateTime.UtcNow;
        var ends = plan.PeriodType == SubscriptionPeriodType.Monthly
            ? starts.AddMonths(1)
            : starts.AddYears(1);

        var subscription = new MemberSubscription
        {
            Id = Guid.NewGuid(),
            MemberId = request.MemberId,
            PlanId = request.PlanId,
            NextPlanId = request.NextPlanId,
            StartsAtUtc = starts,
            EndsAtUtc = ends,
            Status = MembershipStatus.Active,
            AutoRenew = request.AutoRenew
        };

        _dbContext.MemberSubscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(subscription);
    }

    public async Task<MemberSubscriptionDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _dbContext.MemberSubscriptions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Subscription not found.");

        subscription.Status = MembershipStatus.Cancelled;
        subscription.CancelRequestedAtUtc = DateTime.UtcNow;
        subscription.AutoRenew = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(subscription);
    }

    public async Task<MemberSubscriptionDto> EnableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _dbContext.MemberSubscriptions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Subscription not found.");

        subscription.AutoRenew = true;
        subscription.Status = MembershipStatus.Active;
        subscription.CancelRequestedAtUtc = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(subscription);
    }

    public async Task<MemberSubscriptionDto> DisableAutoRenewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _dbContext.MemberSubscriptions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Subscription not found.");

        subscription.AutoRenew = false;
        subscription.CancelRequestedAtUtc = DateTime.UtcNow;
        // Status blijft Active — abonnement loopt door tot EndsAtUtc

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(subscription);
    }

    private static MemberSubscriptionDto MapToDto(MemberSubscription x) => new()
    {
        Id = x.Id,
        MemberId = x.MemberId,
        PlanId = x.PlanId,
        NextPlanId = x.NextPlanId,
        StartsAtUtc = x.StartsAtUtc,
        EndsAtUtc = x.EndsAtUtc,
        Status = x.Status.ToString(),
        AutoRenew = x.AutoRenew,
        CancelRequestedAtUtc = x.CancelRequestedAtUtc
    };
}