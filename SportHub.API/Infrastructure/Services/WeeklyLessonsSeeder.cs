using Microsoft.EntityFrameworkCore;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Infrastructure.Services;

public class WeeklyLessonsSeeder
{
    private readonly AppDbContext _context;

    public WeeklyLessonsSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Ensure GPS columns exist
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Locations ADD COLUMN Latitude DOUBLE NULL");
        }
        catch { /* Column might already exist */ }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Locations ADD COLUMN Longitude DOUBLE NULL");
        }
        catch { /* Column might already exist */ }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Locations ADD COLUMN GpsRadiusMeters DOUBLE NULL DEFAULT 100");
        }
        catch { /* Column might already exist */ }

        var locations = await _context.Locations.ToListAsync();
        var workouts = await _context.Workouts.ToListAsync();
        var instructors = await _context.Instructors.ToListAsync();
        var members = await _context.Members.ToListAsync();

        if (!locations.Any() || !workouts.Any() || !instructors.Any()) return;

        var lessons = new List<Lesson>();
        var reservations = new List<LessonReservation>();
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var endDate = tomorrow.AddDays(21);

        // Delete old lessons beyond next 21 days to prevent duplicates
        var oldLessons = await _context.Lessons
            .Where(l => l.StartTimeUtc > endDate.AddDays(1))
            .ToListAsync();
        if (oldLessons.Any())
        {
            _context.Lessons.RemoveRange(oldLessons);
            await _context.SaveChangesAsync();
        }

        // Create lessons for next 21 days (3 weeks) starting from tomorrow
        for (int day = 0; day < 21; day++)
        {
            var lessonDate = tomorrow.AddDays(day);
            var hours = new[] { 6, 9, 12, 15, 18, 20 };

            foreach (var hour in hours)
            {
                var startTime = lessonDate.AddHours(hour);
                var location = locations[day % locations.Count];
                var workout = workouts[day % workouts.Count];
                var instructor = instructors[day % instructors.Count];

                // Create 2 full lessons and 4 normal lessons per day
                var capacity = (day % 6) < 2 ? 3 : 20; // Small capacity for testing waitlist

                var lesson = new Lesson
                {
                    Id = Guid.NewGuid(),
                    InstructorId = instructor.Id,
                    LocationId = location.Id,
                    WorkoutId = workout.Id,
                    StartTimeUtc = startTime,
                    DurationMinutes = workout.DefaultDurationMinutes,
                    CapacityOverride = capacity,
                    IsInstructorTbd = false,
                    IsCancelled = false
                };

                lessons.Add(lesson);

                // Fill up the small lessons with reservations so users get waitlisted
                if (capacity == 3 && members.Any())
                {
                    for (int i = 0; i < capacity; i++)
                    {
                        var member = members[i % members.Count];
                        var reservation = new LessonReservation
                        {
                            Id = Guid.NewGuid(),
                            MemberId = member.Id,
                            LessonId = lesson.Id,
                            Status = LessonReservationStatus.Reserved,
                            CreatedAtUtc = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 10))
                        };
                        reservations.Add(reservation);
                    }
                }
            }
        }

        await _context.Lessons.AddRangeAsync(lessons);
        await _context.SaveChangesAsync();

        await _context.LessonReservations.AddRangeAsync(reservations);
        await _context.SaveChangesAsync();
    }
}
