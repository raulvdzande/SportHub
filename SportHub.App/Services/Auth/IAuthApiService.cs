using SportHub.Shared.DTOs.Auth;
using SportHub.Shared.DTOs.Members;

namespace SportHub.App.Services.Auth;

public interface IAuthApiService
{
    Task<MemberDto?> LoginAsync(LoginMemberRequestDto request, CancellationToken cancellationToken = default);
    Task LogoutAsync();
    Task<MemberDto?> RestoreSessionAsync(CancellationToken cancellationToken = default);
    Task<MemberDto> CreateAccountAsync(CreateMemberRequestDto request, CancellationToken cancellationToken = default);
}

