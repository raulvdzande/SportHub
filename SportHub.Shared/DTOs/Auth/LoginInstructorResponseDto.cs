namespace SportHub.Shared.DTOs.Auth;

public class LoginInstructorResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public Guid InstructorId { get; set; }
    public string FullName { get; set; } = string.Empty;
}
