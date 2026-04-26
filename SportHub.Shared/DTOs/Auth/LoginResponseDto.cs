namespace SportHub.Shared.DTOs.Auth;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public StaffUserDto User { get; set; } = new();
}

