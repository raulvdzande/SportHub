using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SportHub.API.Configuration;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Infrastructure.Services;

public class StaffUserSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<StaffUser> _passwordHasher;
    private readonly SeedStaffUserOptions _options;

    public StaffUserSeeder(
        AppDbContext dbContext,
        IPasswordHasher<StaffUser> passwordHasher,
        IOptions<SeedStaffUserOptions> options)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _options = options.Value;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
        {
            return;
        }

        var normalizedEmail = _options.Email.Trim().ToLowerInvariant();
        var alreadyExists = await _dbContext.StaffUsers.AnyAsync(x => x.Email.ToLower() == normalizedEmail, cancellationToken);
        if (alreadyExists)
        {
            return;
        }

        var user = new StaffUser
        {
            Id = Guid.NewGuid(),
            Email = _options.Email.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(_options.DisplayName) ? "Medewerker" : _options.DisplayName.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, _options.Password);

        _dbContext.StaffUsers.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

