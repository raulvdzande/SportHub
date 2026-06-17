using SportHub.Shared.DTOs.Auth;

namespace SportHub.API.Application.Interfaces;

public interface IInstructorAuthService
{
    Task<LoginInstructorResponseDto?> LoginAsync(LoginInstructorRequestDto request, CancellationToken cancellationToken = default);
}
