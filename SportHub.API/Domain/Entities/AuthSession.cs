namespace SportHub.API.Domain.Entities;

public class AuthSession
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }

    public Member Member { get; set; } = null!;
}

