using Microsoft.AspNetCore.Http;

namespace SportHub.API.Application.Interfaces;

public interface IPhotoStorageService
{
    Task<string?> SaveInstructorPhotoAsync(IFormFile? photo, CancellationToken cancellationToken = default);
}

