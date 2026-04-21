using Microsoft.EntityFrameworkCore;
using SportHub.API.Domain.Entities;

namespace SportHub.API.Infrastructure.Data.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<LessonRecurrenceRule> LessonRecurrenceRules => Set<LessonRecurrenceRule>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<MemberSubscription> MemberSubscriptions => Set<MemberSubscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<LessonReservation> LessonReservations => Set<LessonReservation>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}