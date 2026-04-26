using Microsoft.EntityFrameworkCore;
using SportHub.Shared.DTOs.Locations;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _dbContext;

    public LocationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Capacity = x.Capacity,
                IsSpinningRoom = x.IsSpinningRoom,
                SpinningBikeCount = x.SpinningBikeCount,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<LocationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await _dbContext.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Location not found.");

        return MapToDto(location);
    }

    public async Task<LocationDto> CreateAsync(CreateLocationRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateSpinningBikeCount(request.IsSpinningRoom, request.SpinningBikeCount, request.Capacity);
        await EnsureUniqueNameAsync(request.Name, cancellationToken);

        var location = new Location
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Capacity = request.Capacity,
            IsSpinningRoom = request.IsSpinningRoom,
            SpinningBikeCount = request.IsSpinningRoom ? request.SpinningBikeCount : null,
            IsActive = request.IsActive
        };

        _dbContext.Locations.Add(location);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(location);
    }

    public async Task<LocationDto> UpdateAsync(Guid id, UpdateLocationRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateSpinningBikeCount(request.IsSpinningRoom, request.SpinningBikeCount, request.Capacity);

        var location = await _dbContext.Locations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Location not found.");

        await EnsureUniqueNameAsync(request.Name, cancellationToken, id);

        location.Name = request.Name.Trim();
        location.Description = request.Description?.Trim();
        location.Capacity = request.Capacity;
        location.IsSpinningRoom = request.IsSpinningRoom;
        location.SpinningBikeCount = request.IsSpinningRoom ? request.SpinningBikeCount : null;
        location.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(location);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await _dbContext.Locations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Location not found.");

        _dbContext.Locations.Remove(location);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Location cannot be removed while lessons are linked to it.");
        }
    }

    private async Task EnsureUniqueNameAsync(string name, CancellationToken cancellationToken, Guid? excludeId = null)
    {
        var normalized = name.Trim().ToLowerInvariant();

        var exists = await _dbContext.Locations.AnyAsync(
            x => x.Name.ToLower() == normalized && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("A location with this name already exists.");
        }
    }

    private static void ValidateSpinningBikeCount(bool isSpinningRoom, int? spinningBikeCount, int capacity)
    {
        if (!isSpinningRoom)
        {
            return;
        }

        if (!spinningBikeCount.HasValue)
        {
            throw new InvalidOperationException("Spinning bike count is required for spinning rooms.");
        }

        if (spinningBikeCount.Value > capacity)
        {
            throw new InvalidOperationException("Spinning bike count cannot exceed location capacity.");
        }
    }

    private static LocationDto MapToDto(Location location)
    {
        return new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Capacity = location.Capacity,
            IsSpinningRoom = location.IsSpinningRoom,
            SpinningBikeCount = location.SpinningBikeCount,
            IsActive = location.IsActive
        };
    }
}
