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
        // Status stays Active - the subscription keeps running until EndsAtUtc

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(subscription);
    }

    public async Task<SubscriptionUpgradeQuoteDto> GetUpgradeQuoteAsync(Guid currentSubscriptionId, Guid targetPlanId, DateTime? calculatedAtUtc = null, CancellationToken cancellationToken = default)
    {
        var now = calculatedAtUtc ?? DateTime.UtcNow;

        var currentSubscription = await _dbContext.MemberSubscriptions
            .AsNoTracking()
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.Id == currentSubscriptionId, cancellationToken)
            ?? throw new KeyNotFoundException("Subscription not found.");

        var targetPlan = await _dbContext.MembershipPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == targetPlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Membership plan not found.");

        if (!string.Equals(currentSubscription.Plan.Currency, targetPlan.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Currency mismatch between current subscription and target plan.");
        }

        var totalDuration = currentSubscription.EndsAtUtc - currentSubscription.StartsAtUtc;
        var remainingDuration = currentSubscription.EndsAtUtc - now;

        var remainingRatio = 0d;
        if (totalDuration.TotalSeconds > 0)
        {
            remainingRatio = Math.Clamp(remainingDuration.TotalSeconds / totalDuration.TotalSeconds, 0d, 1d);
        }

        var remainingCredit = Math.Round(currentSubscription.Plan.Price * (decimal)remainingRatio, 2, MidpointRounding.AwayFromZero);
        var amountToPay = Math.Max(0m, Math.Round(targetPlan.Price - remainingCredit, 2, MidpointRounding.AwayFromZero));

        return new SubscriptionUpgradeQuoteDto
        {
            CurrentSubscriptionId = currentSubscription.Id,
            CurrentPlanId = currentSubscription.PlanId,
            CurrentPlanName = currentSubscription.Plan.Name,
            CurrentPlanPrice = currentSubscription.Plan.Price,
            CurrentSubscriptionEndsAtUtc = currentSubscription.EndsAtUtc,
            TargetPlanId = targetPlan.Id,
            TargetPlanName = targetPlan.Name,
            TargetPlanPrice = targetPlan.Price,
            RemainingCredit = remainingCredit,
            AmountToPay = amountToPay,
            Currency = targetPlan.Currency,
            RemainingRatio = remainingRatio,
            RemainingDays = Math.Max(0, (int)Math.Ceiling((currentSubscription.EndsAtUtc - now).TotalDays)),
            CalculatedAtUtc = now
        };
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