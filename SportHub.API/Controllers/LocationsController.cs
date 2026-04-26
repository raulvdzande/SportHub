using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Locations;
using SportHub.API.Application.Interfaces;

namespace SportHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LocationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var locations = await _locationService.GetAllAsync(cancellationToken);
        return Ok(locations);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var location = await _locationService.GetByIdAsync(id, cancellationToken);
        return Ok(location);
    }

    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create([FromBody] CreateLocationRequestDto request, CancellationToken cancellationToken)
    {
        var location = await _locationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = location.Id }, location);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LocationDto>> Update(Guid id, [FromBody] UpdateLocationRequestDto request, CancellationToken cancellationToken)
    {
        var location = await _locationService.UpdateAsync(id, request, cancellationToken);
        return Ok(location);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _locationService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
