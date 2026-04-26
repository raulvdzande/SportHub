using SportHub.Shared.DTOs.Locations;

namespace SportHub.API.Application.Interfaces;

public interface ILocationService
{
    Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LocationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LocationDto> CreateAsync(CreateLocationRequestDto request, CancellationToken cancellationToken = default);
    Task<LocationDto> UpdateAsync(Guid id, UpdateLocationRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
