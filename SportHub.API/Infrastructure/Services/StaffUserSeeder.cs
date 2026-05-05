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
        var displayName = string.IsNullOrWhiteSpace(_options.DisplayName) ? "Medewerker" : _options.DisplayName.Trim();
        var existingUser = await _dbContext.StaffUsers.FirstOrDefaultAsync(
            x => x.Email.ToLower() == normalizedEmail,
            cancellationToken);

        if (existingUser is null)
        {
            var newUser = new StaffUser
            {
                Id = Guid.NewGuid(),
                Email = _options.Email.Trim(),
                DisplayName = displayName,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, _options.Password);
            _dbContext.StaffUsers.Add(newUser);
        }
        else
        {
            existingUser.Email = _options.Email.Trim();
            existingUser.DisplayName = displayName;
            existingUser.IsActive = true;
            // Keep seeded credentials in sync after DB/provider migrations.
            existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, _options.Password);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

