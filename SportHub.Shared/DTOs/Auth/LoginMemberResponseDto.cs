namespace SportHub.Shared.DTOs.Auth;

public class LoginMemberResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public Guid MemberId { get; set; }
}
