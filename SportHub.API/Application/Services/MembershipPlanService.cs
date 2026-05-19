using Microsoft.EntityFrameworkCore;
using SportHub.Shared.DTOs.Members;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class MembershipPlanService : IMembershipPlanService
{
    private readonly AppDbContext _dbContext;

    public MembershipPlanService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<MembershipPlanDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.MembershipPlans
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<MembershipPlanDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _dbContext.MembershipPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Membership plan not found.");

        return MapToDto(plan);
    }

    public async Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var periodType = ParsePeriodType(request.PeriodType);
        await EnsureUniqueNameAndPeriodAsync(request.Name, periodType, cancellationToken);

        var plan = new MembershipPlan
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            PeriodType = periodType,
            SessionsPerWeekLimit = request.SessionsPerWeekLimit,
            Price = request.Price,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            IsActive = request.IsActive
        };

        _dbContext.MembershipPlans.Add(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(plan);
    }

    public async Task<MembershipPlanDto> UpdateAsync(Guid id, UpdateMembershipPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var periodType = ParsePeriodType(request.PeriodType);
        await EnsureUniqueNameAndPeriodAsync(request.Name, periodType, cancellationToken, id);

        var plan = await _dbContext.MembershipPlans
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Membership plan not found.");

        plan.Name = request.Name.Trim();
        plan.Description = request.Description?.Trim();
        plan.PeriodType = periodType;
        plan.SessionsPerWeekLimit = request.SessionsPerWeekLimit;
        plan.Price = request.Price;
        plan.Currency = request.Currency.Trim().ToUpperInvariant();
        plan.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(plan);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _dbContext.MembershipPlans
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Membership plan not found.");

        _dbContext.MembershipPlans.Remove(plan);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Plan cannot be deleted while subscriptions reference it.");
        }
    }

    private async Task EnsureUniqueNameAndPeriodAsync(string name, SubscriptionPeriodType periodType, CancellationToken cancellationToken, Guid? excludeId = null)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var exists = await _dbContext.MembershipPlans.AnyAsync(
            x => x.Name.ToLower() == normalized
                 && x.PeriodType == periodType
                 && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("A plan with this name and period type already exists.");
        }
    }

    private static SubscriptionPeriodType ParsePeriodType(string value)
    {
        if (!Enum.TryParse<SubscriptionPeriodType>(value.Trim(), true, out var parsed))
        {
            throw new InvalidOperationException("PeriodType must be Monthly or Yearly.");
        }

        return parsed;
    }

    private static MembershipPlanDto MapToDto(MembershipPlan x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Description = x.Description,
        PeriodType = x.PeriodType.ToString(),
        SessionsPerWeekLimit = x.SessionsPerWeekLimit,
        Price = x.Price,
        Currency = x.Currency,
        IsActive = x.IsActive
    };
}
