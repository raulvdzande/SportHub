using SportHub.Shared.DTOs.Members;

namespace SportHub.App.Services.Api;

public interface IMembersApiClient
{
    Task<MemberDto?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<MemberDto?> UpdateCurrentAsync(UpdateMemberProfileRequestDto request, Stream? photo = null, string? photoFileName = null, string? photoContentType = null, CancellationToken cancellationToken = default);
}

