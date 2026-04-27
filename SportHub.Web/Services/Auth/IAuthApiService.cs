using SportHub.Shared.DTOs.Auth;

namespace SportHub.Web.Services.Auth;

public interface IAuthApiService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task LogoutAsync();
}

