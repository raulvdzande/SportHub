using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportHub.Shared.DTOs.Auth;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class InstructorAuthService : IInstructorAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<Instructor> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<InstructorAuthService> _logger;

    public InstructorAuthService(
        AppDbContext dbContext,
        IPasswordHasher<Instructor> passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<InstructorAuthService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<LoginInstructorResponseDto?> LoginAsync(LoginInstructorRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalized = request.Email.Trim().ToLowerInvariant();
        var instructor = await _dbContext.Instructors.FirstOrDefaultAsync(x => x.Email.ToLower() == normalized, cancellationToken);
        if (instructor is null || !instructor.IsActive || string.IsNullOrEmpty(instructor.PasswordHash))
        {
            _logger.LogWarning("Login attempt failed: instructor not found or inactive for email {Email}", normalized);
            return null;
        }

        var verification = _passwordHasher.VerifyHashedPassword(instructor, instructor.PasswordHash ?? string.Empty, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login attempt failed: invalid password for instructor {InstructorId}", instructor.Id);
            return null;
        }

        var token = _jwtTokenService.GenerateTokenForInstructor(instructor);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginInstructorResponseDto
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            InstructorId = instructor.Id,
            FullName = instructor.FullName
        };
    }
}
