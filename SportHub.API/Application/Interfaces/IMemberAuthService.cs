using SportHub.Shared.DTOs.Auth;

namespace SportHub.API.Application.Interfaces;

public interface IMemberAuthService
{
    Task<LoginMemberResponseDto?> LoginAsync(LoginMemberRequestDto request, CancellationToken cancellationToken = default);
}
