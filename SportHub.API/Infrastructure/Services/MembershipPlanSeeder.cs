using Microsoft.EntityFrameworkCore;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Infrastructure.Services;

public class MembershipPlanSeeder
{
    private readonly AppDbContext _db;

    public MembershipPlanSeeder(AppDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Only seed if there are no active plans
        if (await _db.MembershipPlans.AnyAsync(p => p.IsActive, cancellationToken))
            return;

        var plans = new[]
        {
            new MembershipPlan
            {
                Id          = new Guid("11111111-0000-0000-0000-000000000001"),
                Name        = "Basis",
                Description = "2 lessen per week",
                PeriodType  = SubscriptionPeriodType.Monthly,
                SessionsPerWeekLimit = 2,
                Price    = 29.99m,
                Currency = "EUR",
                IsActive = true
            },
            new MembershipPlan
            {
                Id          = new Guid("11111111-0000-0000-0000-000000000002"),
                Name        = "Onbeperkt",
                Description = "Onbeperkt lessen per week",
                PeriodType  = SubscriptionPeriodType.Monthly,
                SessionsPerWeekLimit = null,
                Price    = 49.99m,
                Currency = "EUR",
                IsActive = true
            },
            new MembershipPlan
            {
                Id          = new Guid("11111111-0000-0000-0000-000000000003"),
                Name        = "Jaarplan Basis",
                Description = "2 lessen per week – jaarprijs",
                PeriodType  = SubscriptionPeriodType.Yearly,
                SessionsPerWeekLimit = 2,
                Price    = 299.99m,
                Currency = "EUR",
                IsActive = true
            },
            new MembershipPlan
            {
                Id          = new Guid("11111111-0000-0000-0000-000000000004"),
                Name        = "Jaarplan Onbeperkt",
                Description = "Onbeperkt lessen – jaarprijs",
                PeriodType  = SubscriptionPeriodType.Yearly,
                SessionsPerWeekLimit = null,
                Price    = 499.99m,
                Currency = "EUR",
                IsActive = true
            }
        };

        _db.MembershipPlans.AddRange(plans);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
