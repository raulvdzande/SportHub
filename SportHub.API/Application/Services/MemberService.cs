using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportHub.Shared.DTOs.Members;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class MemberService : IMemberService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<Member> _passwordHasher;
    private readonly SportHub.API.Application.Interfaces.IEmailService _emailService;

    public MemberService(AppDbContext dbContext, IPasswordHasher<Member> passwordHasher, SportHub.API.Application.Interfaces.IEmailService emailService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<IReadOnlyCollection<MemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Members
            .AsNoTracking()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => MapToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<MemberDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");

        return MapToDto(member);
    }

    public async Task<MemberDto> CreateAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        await EnsureUniqueEmailAndUsernameAsync(request.Email, request.Username, cancellationToken);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Username = request.Username?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        member.PasswordHash = _passwordHasher.HashPassword(member, request.Password);

        _dbContext.Members.Add(member);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Send a basic welcome/confirmation email (development console)
        var subject = "Welkom bij SportHub - bevestiging registratie";
        var body = $"<p>Beste {member.FirstName},</p><p>Bedankt voor je aanmelding bij SportHub. Je account is aangemaakt met e-mail {member.Email}.</p>";
        try
        {
            await _emailService.SendEmailAsync(member.Email, subject, body, cancellationToken);
        }
        catch
        {
            // swallow email errors for now to not block registration
        }

        return MapToDto(member);
    }

    public async Task<MemberDto> UpdateAsync(Guid id, UpdateMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        await EnsureUniqueEmailAndUsernameAsync(request.Email, request.Username, cancellationToken, id);

        var member = await _dbContext.Members
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");

        member.Email = request.Email.Trim();
        member.FirstName = request.FirstName.Trim();
        member.LastName = request.LastName.Trim();
        member.Username = request.Username?.Trim();
        member.PhoneNumber = request.PhoneNumber?.Trim();
        member.ProfilePhotoUrl = request.ProfilePhotoUrl?.Trim();
        member.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(member);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.Members
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Member not found.");

        _dbContext.Members.Remove(member);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Member cannot be deleted while subscriptions or payments exist.");
        }
    }

    private async Task EnsureUniqueEmailAndUsernameAsync(string email, string? username, CancellationToken cancellationToken, Guid? excludeId = null)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await _dbContext.Members.AnyAsync(
                x => x.Email.ToLower() == normalizedEmail && (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken))
        {
            throw new InvalidOperationException("A member with this email already exists.");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        var normalizedUser = username.Trim().ToLowerInvariant();
        if (await _dbContext.Members.AnyAsync(
                x => x.Username != null && x.Username.ToLower() == normalizedUser && (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken))
        {
            throw new InvalidOperationException("A member with this username already exists.");
        }
    }

    private static MemberDto MapToDto(Member x) => new()
    {
        Id = x.Id,
        Email = x.Email,
        FirstName = x.FirstName,
        LastName = x.LastName,
        Username = x.Username,
        PhoneNumber = x.PhoneNumber,
        ProfilePhotoUrl = x.ProfilePhotoUrl,
        IsActive = x.IsActive,
        CreatedAtUtc = x.CreatedAtUtc
    };
}
