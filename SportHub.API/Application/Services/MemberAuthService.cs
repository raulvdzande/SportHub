using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportHub.Shared.DTOs.Auth;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class MemberAuthService : IMemberAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<Member> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public MemberAuthService(AppDbContext dbContext, IPasswordHasher<Member> passwordHasher, IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginMemberResponseDto?> LoginAsync(LoginMemberRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalized = request.Email.Trim().ToLowerInvariant();
        var member = await _dbContext.Members.FirstOrDefaultAsync(x => x.Email.ToLower() == normalized, cancellationToken);
        if (member is null || !member.IsActive)
        {
            return null;
        }

        var verification = _passwordHasher.VerifyHashedPassword(member, member.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = _jwtTokenService.GenerateTokenForMember(member);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginMemberResponseDto
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            MemberId = member.Id
        };
    }
}
