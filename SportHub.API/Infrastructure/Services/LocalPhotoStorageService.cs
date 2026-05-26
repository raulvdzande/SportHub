using Microsoft.AspNetCore.Http;
using SportHub.API.Application.Interfaces;

namespace SportHub.API.Infrastructure.Services;

public class LocalPhotoStorageService : IPhotoStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private readonly IWebHostEnvironment _environment;

    public LocalPhotoStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public Task<string?> SaveInstructorPhotoAsync(IFormFile? photo, CancellationToken cancellationToken = default)
        => SavePhotoAsync(photo, "instructors", cancellationToken);

    public Task<string?> SaveMemberPhotoAsync(IFormFile? photo, CancellationToken cancellationToken = default)
        => SavePhotoAsync(photo, "members", cancellationToken);

    private async Task<string?> SavePhotoAsync(IFormFile? photo, string folderName, CancellationToken cancellationToken)
    {
        if (photo is null || photo.Length == 0)
        {
            return null;
        }

        var extension = Path.GetExtension(photo.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported photo format. Use jpg, png or webp.");
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadFolder = Path.Combine(webRoot, "uploads", folderName);
        Directory.CreateDirectory(uploadFolder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(uploadFolder, fileName);

        await using var stream = File.Create(fullPath);
        await photo.CopyToAsync(stream, cancellationToken);

        return $"/uploads/{folderName}/{fileName}";
    }
}
