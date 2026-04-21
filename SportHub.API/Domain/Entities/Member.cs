namespace SportHub.API.Domain.Entities;

public class Member
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<MemberSubscription> Subscriptions { get; set; } = new List<MemberSubscription>();
    public ICollection<LessonReservation> LessonReservations { get; set; } = new List<LessonReservation>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
    public ICollection<AuthSession> AuthSessions { get; set; } = new List<AuthSession>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

