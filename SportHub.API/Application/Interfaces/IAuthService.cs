using SportHub.API.Application.DTOs.Auth;

namespace SportHub.API.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}

