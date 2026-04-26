using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Instructors;
using SportHub.API.Application.Interfaces;
using SportHub.API.Controllers.Requests;

namespace SportHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _instructorService;
    private readonly IPhotoStorageService _photoStorageService;

    public InstructorsController(IInstructorService instructorService, IPhotoStorageService photoStorageService)
    {
        _instructorService = instructorService;
        _photoStorageService = photoStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InstructorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var instructors = await _instructorService.GetAllAsync(cancellationToken);
        return Ok(instructors);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstructorDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var instructor = await _instructorService.GetByIdAsync(id, cancellationToken);
        return Ok(instructor);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<InstructorDto>> Create([FromForm] CreateInstructorRequest request, CancellationToken cancellationToken)
    {
        var photoUrl = await _photoStorageService.SaveInstructorPhotoAsync(request.Photo, cancellationToken);

        var instructor = await _instructorService.CreateAsync(new CreateInstructorRequestDto
        {
            FullName = request.FullName,
            PhotoUrl = photoUrl,
            IsTbd = request.IsTbd,
            IsActive = request.IsActive
        }, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = instructor.Id }, instructor);
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<InstructorDto>> Update(Guid id, [FromForm] UpdateInstructorRequest request, CancellationToken cancellationToken)
    {
        var existing = await _instructorService.GetByIdAsync(id, cancellationToken);
        var photoUrl = await _photoStorageService.SaveInstructorPhotoAsync(request.Photo, cancellationToken) ?? existing.PhotoUrl;

        var instructor = await _instructorService.UpdateAsync(id, new UpdateInstructorRequestDto
        {
            FullName = request.FullName,
            PhotoUrl = photoUrl,
            IsTbd = request.IsTbd,
            IsActive = request.IsActive
        }, cancellationToken);

        return Ok(instructor);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _instructorService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
