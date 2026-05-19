using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SportHub.Shared.DTOs.Instructors;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class InstructorService : IInstructorService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<Instructor> _passwordHasher;

    public InstructorService(AppDbContext dbContext, IPasswordHasher<Instructor> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyCollection<InstructorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Instructors
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .Select(x => new InstructorDto
            {
                Id = x.Id,
                FullName = x.FullName,
                PhotoUrl = x.PhotoUrl,
                IsTbd = x.IsTbd,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InstructorDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Instructor not found.");

        return MapToDto(instructor);
    }

    public async Task<InstructorDto> CreateAsync(CreateInstructorRequestDto request, CancellationToken cancellationToken = default)
    {
        var instructor = new Instructor
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            PhotoUrl = request.PhotoUrl,
            IsTbd = request.IsTbd,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            await EnsureEmailUniqueAsync(request.Email, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            instructor.PasswordHash = _passwordHasher.HashPassword(instructor, request.Password);
        }

        _dbContext.Instructors.Add(instructor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(instructor);
    }

    public async Task<InstructorDto> UpdateAsync(Guid id, UpdateInstructorRequestDto request, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Instructor not found.");

        instructor.FullName = request.FullName.Trim();
        instructor.Email = request.Email?.Trim();
        instructor.PhoneNumber = request.PhoneNumber?.Trim();
        instructor.PhotoUrl = request.PhotoUrl;
        instructor.IsTbd = request.IsTbd;
        instructor.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            await EnsureEmailUniqueAsync(request.Email, cancellationToken, id);
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            instructor.PasswordHash = _passwordHasher.HashPassword(instructor, request.Password);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(instructor);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var instructor = await _dbContext.Instructors
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Instructor not found.");

        _dbContext.Instructors.Remove(instructor);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static InstructorDto MapToDto(Instructor instructor)
    {
        return new InstructorDto
        {
            Id = instructor.Id,
            FullName = instructor.FullName,
            Email = instructor.Email,
            PhoneNumber = instructor.PhoneNumber,
            PhotoUrl = instructor.PhotoUrl,
            IsTbd = instructor.IsTbd,
            IsActive = instructor.IsActive,
            CreatedAtUtc = instructor.CreatedAtUtc
        };
    }

    private async Task EnsureEmailUniqueAsync(string email, CancellationToken cancellationToken, Guid? excludeId = null)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = await _dbContext.Instructors.AnyAsync(
            x => x.Email != null
                 && x.Email.ToLower() == normalizedEmail
                 && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("An instructor with this email already exists.");
        }
    }
}
