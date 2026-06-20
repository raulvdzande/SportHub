using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Infrastructure.Services;

public class ComprehensiveDataSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<Member> _memberHasher;
    private readonly IPasswordHasher<Instructor> _instructorHasher;

    public ComprehensiveDataSeeder(
        AppDbContext context,
        IPasswordHasher<Member> memberHasher,
        IPasswordHasher<Instructor> instructorHasher)
    {
        _context = context;
        _memberHasher = memberHasher;
        _instructorHasher = instructorHasher;
    }

    public async Task SeedAsync()
    {
        // Always seed locations to ensure only gyms exist (deletes old "Zaal 1" etc)
        await SeedLocationsAsync();

        // Only seed other data if they don't exist
        if (await _context.Lessons.AnyAsync()) return;

        await SeedWorkoutsAsync();
        await SeedInstructorsAsync();
        await SeedMembersAsync();
        await SeedMembershipPlansAsync();
        await SeedLessonsAsync();
        await SeedMemberSubscriptionsAsync();
        await SeedLessonReservationsAsync();
        await SeedCheckInsAsync();
        await SeedNotificationsAsync();
    }

    private async Task SeedLocationsAsync()
    {
        var gymLocations = new List<Location>
        {
            new() { Id = Guid.NewGuid(), Name = "Gym Amsterdam Central", Description = "Central Amsterdam location", Capacity = 100, IsSpinningRoom = true, SpinningBikeCount = 20, IsActive = true, Latitude = 52.3696, Longitude = 4.9052 },
            new() { Id = Guid.NewGuid(), Name = "Gym Amsterdam Zuid", Description = "South Amsterdam location", Capacity = 80, IsSpinningRoom = false, IsActive = true, Latitude = 52.3599, Longitude = 4.8866 },
            new() { Id = Guid.NewGuid(), Name = "Gym Utrecht", Description = "Utrecht location", Capacity = 60, IsSpinningRoom = true, SpinningBikeCount = 15, IsActive = true, Latitude = 52.0907, Longitude = 5.1214 },
            new() { Id = Guid.NewGuid(), Name = "Gym Rotterdam", Description = "Rotterdam location", Capacity = 70, IsSpinningRoom = false, IsActive = true, Latitude = 51.9225, Longitude = 4.4792 },
            new() { Id = Guid.NewGuid(), Name = "Gym Den Haag", Description = "The Hague location", Capacity = 50, IsSpinningRoom = true, SpinningBikeCount = 10, IsActive = true, Latitude = 52.0705, Longitude = 4.3007 },
            new() { Id = Guid.NewGuid(), Name = "Gym Veghel", Description = "Dorshout 8, Veghel", Capacity = 40, IsSpinningRoom = true, SpinningBikeCount = 12, IsActive = true, Latitude = 51.5395, Longitude = 5.2706 },
            new() { Id = Guid.NewGuid(), Name = "Gym Breda", Description = "Hogeschoollaan 1, Breda", Capacity = 60, IsSpinningRoom = true, SpinningBikeCount = 16, IsActive = true, Latitude = 51.5750, Longitude = 4.7839 },
        };

        // Check if only gyms exist
        var allLocations = await _context.Locations.ToListAsync();
        var nonGymLocations = allLocations.Where(l => !l.Name.StartsWith("Gym ")).ToList();

        if (nonGymLocations.Any())
        {
            // First, delete lessons that reference non-gym locations (cascading delete)
            var nonGymLocationIds = nonGymLocations.Select(l => l.Id).ToList();
            var lessonsToDelete = await _context.Lessons
                .Where(l => nonGymLocationIds.Contains(l.LocationId))
                .ToListAsync();

            if (lessonsToDelete.Any())
            {
                _context.Lessons.RemoveRange(lessonsToDelete);
                await _context.SaveChangesAsync();
            }

            // Then delete the non-gym locations
            _context.Locations.RemoveRange(nonGymLocations);
            await _context.SaveChangesAsync();
        }

        // Only add gyms if they don't exist
        var existingGyms = await _context.Locations.Where(l => l.Name.StartsWith("Gym ")).ToListAsync();
        if (!existingGyms.Any())
        {
            await _context.Locations.AddRangeAsync(gymLocations);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedWorkoutsAsync()
    {
        var workouts = new List<Workout>
        {
            new() { Id = Guid.NewGuid(), Name = "Spinning", Description = "High-intensity cycling workout", DefaultDurationMinutes = 45, IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Yoga", Description = "Relaxing yoga and meditation", DefaultDurationMinutes = 60, IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "CrossFit", Description = "Functional fitness workout", DefaultDurationMinutes = 50, IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Pilates", Description = "Core strengthening workout", DefaultDurationMinutes = 45, IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Zumba", Description = "Dance fitness workout", DefaultDurationMinutes = 45, IsActive = true, CreatedAtUtc = DateTime.UtcNow },
        };

        await _context.Workouts.AddRangeAsync(workouts);
        await _context.SaveChangesAsync();
    }

    private async Task SeedInstructorsAsync()
    {
        var instructors = new List<Instructor>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "instructor@test.com",
                FullName = "Test Instructor",
                PhoneNumber = "0687654321",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                PasswordHash = _instructorHasher.HashPassword(null!, "Password123!")
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "jan.jansen@gymnasium.nl",
                FullName = "Jan Jansen",
                PhoneNumber = "0612345678",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                PasswordHash = _instructorHasher.HashPassword(null!, "SecurePass123!")
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "marie.hendriks@gymnasium.nl",
                FullName = "Marie Hendriks",
                PhoneNumber = "0687654321",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                PasswordHash = _instructorHasher.HashPassword(null!, "SecurePass123!")
            },
        };

        await _context.Instructors.AddRangeAsync(instructors);
        await _context.SaveChangesAsync();
    }

    private async Task SeedMembersAsync()
    {
        var members = new List<Member>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "member@test.com",
                FirstName = "Test",
                LastName = "Member",
                PhoneNumber = "0612345678",
                CreatedAtUtc = DateTime.UtcNow,
                PasswordHash = _memberHasher.HashPassword(null!, "Password123!")
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "pieter.vandijk@email.com",
                FirstName = "Pieter",
                LastName = "van Dijk",
                PhoneNumber = "0612345679",
                CreatedAtUtc = DateTime.UtcNow,
                PasswordHash = _memberHasher.HashPassword(null!, "SecurePass123!")
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "lisa.dewilde@email.com",
                FirstName = "Lisa",
                LastName = "de Wilde",
                PhoneNumber = "0612345680",
                CreatedAtUtc = DateTime.UtcNow,
                PasswordHash = _memberHasher.HashPassword(null!, "SecurePass123!")
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "tom.bakker@email.com",
                FirstName = "Tom",
                LastName = "Bakker",
                PhoneNumber = "0612345681",
                CreatedAtUtc = DateTime.UtcNow,
                PasswordHash = _memberHasher.HashPassword(null!, "SecurePass123!")
            },
        };

        await _context.Members.AddRangeAsync(members);
        await _context.SaveChangesAsync();
    }

    private async Task SeedMembershipPlansAsync()
    {
        var plans = new List<MembershipPlan>
        {
            new() { Id = Guid.NewGuid(), Name = "Basic", Description = "Access to gym facilities", Price = 29.99m, PeriodType = SubscriptionPeriodType.Monthly, SessionsPerWeekLimit = null, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Premium", Description = "Access + Group classes", Price = 49.99m, PeriodType = SubscriptionPeriodType.Monthly, SessionsPerWeekLimit = 8, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Elite", Description = "Full access + Personal training", Price = 99.99m, PeriodType = SubscriptionPeriodType.Monthly, SessionsPerWeekLimit = null, IsActive = true },
        };

        await _context.MembershipPlans.AddRangeAsync(plans);
        await _context.SaveChangesAsync();
    }

    private async Task SeedLessonsAsync()
    {
        var locations = await _context.Locations.ToListAsync();
        var workouts = await _context.Workouts.ToListAsync();
        var instructors = await _context.Instructors.ToListAsync();

        if (!locations.Any() || !workouts.Any() || !instructors.Any()) return;

        var lessons = new List<Lesson>();
        var today = DateTime.UtcNow.Date.AddDays(1); // Start from tomorrow
        var veghel = locations.FirstOrDefault(l => l.Name == "Gym Veghel");

        // Create lessons for next 21 days (3 weeks)
        for (int day = 0; day < 21; day++)
        {
            var lessonDate = today.AddDays(day);
            var hours = new[] { 6, 9, 12, 15, 18, 20 };

            foreach (var hour in hours)
            {
                var startTime = lessonDate.AddHours(hour);
                var location = locations[day % locations.Count];
                var workout = workouts[day % workouts.Count];
                var instructor = instructors[day % instructors.Count];

                var lesson = new Lesson
                {
                    Id = Guid.NewGuid(),
                    InstructorId = instructor.Id,
                    LocationId = location.Id,
                    WorkoutId = workout.Id,
                    StartTimeUtc = startTime,
                    DurationMinutes = workout.DefaultDurationMinutes,
                    CapacityOverride = 3,
                    IsInstructorTbd = false,
                    IsCancelled = false
                };

                lessons.Add(lesson);
            }
        }

        // Add empty Veghel lessons for this week and next week (14 days)
        if (veghel != null)
        {
            for (int day = 0; day < 14; day++)
            {
                var lessonDate = today.AddDays(day);
                var hours = new[] { 8, 10, 14, 17, 19 };

                foreach (var hour in hours)
                {
                    var startTime = lessonDate.AddHours(hour);
                    var workout = workouts[day % workouts.Count];
                    var instructor = instructors[day % instructors.Count];

                    var lesson = new Lesson
                    {
                        Id = Guid.NewGuid(),
                        InstructorId = instructor.Id,
                        LocationId = veghel.Id,
                        WorkoutId = workout.Id,
                        StartTimeUtc = startTime,
                        DurationMinutes = workout.DefaultDurationMinutes,
                        CapacityOverride = 3,
                        IsInstructorTbd = false,
                        IsCancelled = false
                    };

                    lessons.Add(lesson);
                }
            }
        }

        await _context.Lessons.AddRangeAsync(lessons);
        await _context.SaveChangesAsync();
    }

    private async Task SeedMemberSubscriptionsAsync()
    {
        var members = await _context.Members.ToListAsync();
        var plans = await _context.MembershipPlans.ToListAsync();

        if (!members.Any() || !plans.Any()) return;

        var subscriptions = new List<MemberSubscription>();

        for (int i = 0; i < members.Count; i++)
        {
            var subscription = new MemberSubscription
            {
                Id = Guid.NewGuid(),
                MemberId = members[i].Id,
                PlanId = plans[i % plans.Count].Id,
                StartsAtUtc = DateTime.UtcNow.AddMonths(-1),
                EndsAtUtc = DateTime.UtcNow.AddMonths(11),
                Status = MembershipStatus.Active,
                AutoRenew = true
            };

            subscriptions.Add(subscription);
        }

        await _context.MemberSubscriptions.AddRangeAsync(subscriptions);
        await _context.SaveChangesAsync();
    }

    private async Task SeedLessonReservationsAsync()
    {
        var members = await _context.Members.ToListAsync();
        var locations = await _context.Locations.ToListAsync();
        var veghel = locations.FirstOrDefault(l => l.Name == "Gym Veghel");

        var lessons = await _context.Lessons
            .Include(x => x.Location)
            .ToListAsync();

        if (!members.Any() || !lessons.Any()) return;

        var reservations = new List<LessonReservation>();

        // Create 5 reservations per lesson EXCEPT for Veghel lessons (keep them empty)
        for (int i = 0; i < lessons.Count; i++)
        {
            var lesson = lessons[i];

            // Skip Veghel lessons - keep them empty
            if (veghel != null && lesson.LocationId == veghel.Id)
                continue;

            // Create 5 reservations per lesson to fill capacity (3) and create waitlist
            for (int j = 0; j < 5; j++)
            {
                var member = members[(i * 5 + j) % members.Count];
                var reservationOrder = j; // Position in this lesson (0-4)

                // First 3 get Reserved, rest get Waitlisted
                var status = reservationOrder < 3
                    ? LessonReservationStatus.Reserved
                    : LessonReservationStatus.Waitlisted;

                var reservation = new LessonReservation
                {
                    Id = Guid.NewGuid(),
                    MemberId = member.Id,
                    LessonId = lesson.Id,
                    Status = status,
                    WaitlistPosition = status == LessonReservationStatus.Waitlisted ? (reservationOrder - 2) : null,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
                };

                reservations.Add(reservation);
            }
        }

        await _context.LessonReservations.AddRangeAsync(reservations);
        await _context.SaveChangesAsync();
    }

    private async Task SeedCheckInsAsync()
    {
        var members = await _context.Members.ToListAsync();
        var lessons = await _context.Lessons.Take(10).ToListAsync();

        if (!members.Any() || !lessons.Any()) return;

        var checkIns = new List<CheckIn>();

        for (int i = 0; i < members.Count * 2; i++)
        {
            var member = members[i % members.Count];
            var lesson = lessons[i % lessons.Count];
            var method = (CheckInMethod)(i % 3);

            var checkIn = new CheckIn
            {
                Id = Guid.NewGuid(),
                MemberId = member.Id,
                LessonId = lesson.Id,
                Method = method,
                DeviceIdentifier = method == CheckInMethod.Rfid ? $"RFID-{i:X8}" : null,
                Latitude = method == CheckInMethod.Gps ? 52.3676 + (Random.Shared.NextDouble() - 0.5) * 0.01 : null,
                Longitude = method == CheckInMethod.Gps ? 4.9052 + (Random.Shared.NextDouble() - 0.5) * 0.01 : null,
                CheckedInAtUtc = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60))
            };

            checkIns.Add(checkIn);
        }

        await _context.CheckIns.AddRangeAsync(checkIns);
        await _context.SaveChangesAsync();
    }

    private async Task SeedNotificationsAsync()
    {
        var members = await _context.Members.ToListAsync();

        if (!members.Any()) return;

        var notifications = new List<Notification>();
        var messages = new[]
        {
            "You have successfully checked in",
            "Your reservation is confirmed",
            "You are on the waitlist",
            "Your membership has been renewed",
            "New programme available"
        };

        for (int i = 0; i < members.Count * 5; i++)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                MemberId = members[i % members.Count].Id,
                Title = "SportHub Update",
                Message = messages[i % messages.Length],
                Type = (NotificationType)(i % 3),
                Status = (NotificationStatus)(i % 2),
                CreatedAtUtc = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
                SentAtUtc = i % 2 == 0 ? DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)) : null,
                ReadAtUtc = i % 3 == 0 ? DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 20)) : null
            };

            notifications.Add(notification);
        }

        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();
    }
}
