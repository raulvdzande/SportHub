using Microsoft.EntityFrameworkCore;
using SportHub.Shared.DTOs.Instructors;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class InstructorService : IInstructorService
{
    private readonly AppDbContext _dbContext;

    public InstructorService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
            PhotoUrl = request.PhotoUrl,
            IsTbd = request.IsTbd,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

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
        instructor.PhotoUrl = request.PhotoUrl;
        instructor.IsTbd = request.IsTbd;
        instructor.IsActive = request.IsActive;

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
            PhotoUrl = instructor.PhotoUrl,
            IsTbd = instructor.IsTbd,
            IsActive = instructor.IsActive,
            CreatedAtUtc = instructor.CreatedAtUtc
        };
    }
}
