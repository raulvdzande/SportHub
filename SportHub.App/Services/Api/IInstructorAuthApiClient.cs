using SportHub.Shared.DTOs.Auth;

namespace SportHub.App.Services.Api;

public interface IInstructorAuthApiClient
{
    Task<LoginInstructorResponseDto?> LoginAsync(LoginInstructorRequestDto request, CancellationToken ct = default);
}
