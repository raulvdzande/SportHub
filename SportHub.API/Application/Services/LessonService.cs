using Microsoft.EntityFrameworkCore;
using SportHub.Shared.DTOs.Lessons;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class LessonService : ILessonService
{
    private const int MaxGeneratedLessons = 500;

    private readonly AppDbContext _dbContext;

    public LessonService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.Lessons
            .AsNoTracking()
            .OrderBy(x => x.StartTimeUtc)
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(MapToDto);
    }

    public async Task<LessonDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lesson = await _dbContext.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Lesson not found.");

        return MapToDto(lesson);
    }

    public async Task<LessonDto> CreateAsync(CreateLessonRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateForeignKeysAsync(request.WorkoutId, request.LocationId, request.InstructorId, request.RecurrenceRuleId, cancellationToken);

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            WorkoutId = request.WorkoutId,
            LocationId = request.LocationId,
            InstructorId = request.InstructorId,
            RecurrenceRuleId = request.RecurrenceRuleId,
            StartTimeUtc = request.StartTimeUtc,
            DurationMinutes = request.DurationMinutes,
            CapacityOverride = request.CapacityOverride,
            IsInstructorTbd = request.IsInstructorTbd,
            IsCancelled = false
        };

        _dbContext.Lessons.Add(lesson);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(lesson);
    }

    public async Task<LessonDto> UpdateAsync(Guid id, UpdateLessonRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateForeignKeysAsync(request.WorkoutId, request.LocationId, request.InstructorId, request.RecurrenceRuleId, cancellationToken);

        var lesson = await _dbContext.Lessons
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Lesson not found.");

        lesson.WorkoutId = request.WorkoutId;
        lesson.LocationId = request.LocationId;
        lesson.InstructorId = request.InstructorId;
        lesson.RecurrenceRuleId = request.RecurrenceRuleId;
        lesson.StartTimeUtc = request.StartTimeUtc;
        lesson.DurationMinutes = request.DurationMinutes;
        lesson.CapacityOverride = request.CapacityOverride;
        lesson.IsInstructorTbd = request.IsInstructorTbd;
        lesson.IsCancelled = request.IsCancelled;
        lesson.CancellationReason = request.CancellationReason?.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(lesson);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lesson = await _dbContext.Lessons
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Lesson not found.");

        _dbContext.Lessons.Remove(lesson);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<LessonRecurrenceRuleDto> CreateRecurrenceRuleAsync(CreateLessonRecurrenceRuleRequestDto request, CancellationToken cancellationToken = default)
    {
        var rule = MapToEntity(request);
        _dbContext.LessonRecurrenceRules.Add(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapRuleToDto(rule);
    }

    public async Task<GenerateRecurringLessonsResponseDto> GenerateRecurringLessonsAsync(GenerateRecurringLessonsRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateForeignKeysAsync(request.WorkoutId, request.LocationId, request.InstructorId, null, cancellationToken);

        var rule = MapToEntity(request.Recurrence);
        _dbContext.LessonRecurrenceRules.Add(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var effectiveMax = Math.Min(rule.OccurrenceCount ?? MaxGeneratedLessons, MaxGeneratedLessons);
        var times = ExpandOccurrences(
            request.StartTimeUtc,
            rule.RecurrenceType,
            rule.Interval,
            rule.DaysOfWeekCsv,
            rule.EndDateUtc,
            effectiveMax).ToList();

        var lessonIds = new List<Guid>();
        foreach (var start in times)
        {
            var lesson = new Lesson
            {
                Id = Guid.NewGuid(),
                WorkoutId = request.WorkoutId,
                LocationId = request.LocationId,
                InstructorId = request.InstructorId,
                RecurrenceRuleId = rule.Id,
                StartTimeUtc = start,
                DurationMinutes = request.DurationMinutes,
                CapacityOverride = request.CapacityOverride,
                IsInstructorTbd = request.IsInstructorTbd,
                IsCancelled = false
            };
            _dbContext.Lessons.Add(lesson);
            lessonIds.Add(lesson.Id);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new GenerateRecurringLessonsResponseDto
        {
            RecurrenceRuleId = rule.Id,
            LessonsCreated = lessonIds.Count,
            LessonIds = lessonIds
        };
    }

    public async Task<IReadOnlyCollection<MobileLessonSummaryDto>> GetMobileScheduleAsync(DateTime fromUtc, DateTime toUtc, Guid? instructorId = null, CancellationToken cancellationToken = default)
    {
        var lessons = await _dbContext.Lessons
            .AsNoTracking()
            .Where(x => x.StartTimeUtc >= fromUtc && x.StartTimeUtc <= toUtc
                        && (instructorId == null || x.InstructorId == instructorId))
            .Include(x => x.Workout)
            .Include(x => x.Location)
            .Include(x => x.Instructor)
            .Include(x => x.Reservations)
                .ThenInclude(x => x.Member)
            .OrderBy(x => x.StartTimeUtc)
            .ToListAsync(cancellationToken);

        return lessons.Select(MapToMobileSummaryDto).ToList();
    }

    public async Task<MobileLessonDetailsDto> GetMobileDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lesson = await _dbContext.Lessons
            .AsNoTracking()
            .Include(x => x.Workout)
            .Include(x => x.Location)
            .Include(x => x.Instructor)
            .Include(x => x.Reservations)
                .ThenInclude(x => x.Member)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Lesson not found.");

        // Get check-ins for this lesson
        var checkIns = await _dbContext.CheckIns
            .AsNoTracking()
            .Where(x => x.LessonId == id)
            .ToListAsync(cancellationToken);

        var summary = MapToMobileSummaryDto(lesson);
        return new MobileLessonDetailsDto
        {
            Id = summary.Id,
            WorkoutId = summary.WorkoutId,
            WorkoutName = summary.WorkoutName,
            LocationId = summary.LocationId,
            LocationName = summary.LocationName,
            InstructorId = summary.InstructorId,
            InstructorName = summary.InstructorName,
            StartTimeUtc = summary.StartTimeUtc,
            DurationMinutes = summary.DurationMinutes,
            CapacityOverride = summary.CapacityOverride,
            RegisteredCount = summary.RegisteredCount,
            IsInstructorTbd = summary.IsInstructorTbd,
            IsCancelled = summary.IsCancelled,
            CancellationReason = summary.CancellationReason,
            Participants = lesson.Reservations
                .Where(x => x.Status != LessonReservationStatus.Cancelled)
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x =>
                {
                    var checkIn = checkIns.FirstOrDefault(c => c.MemberId == x.MemberId);
                    return new LessonParticipantDto
                    {
                        MemberId = x.MemberId,
                        DisplayName = BuildMemberDisplayName(x.Member),
                        Username = x.Member.Username,
                        ProfilePhotoUrl = x.Member.ProfilePhotoUrl,
                        ReservationStatus = x.Status.ToString(),
                        HasCheckedIn = checkIn != null,
                        CheckInMethod = checkIn?.Method.ToString()
                    };
                })
                .ToList()
        };
    }

    private async Task ValidateForeignKeysAsync(Guid workoutId, Guid locationId, Guid? instructorId, Guid? recurrenceRuleId, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Workouts.AnyAsync(x => x.Id == workoutId, cancellationToken))
        {
            throw new KeyNotFoundException("Workout not found.");
        }

        if (!await _dbContext.Locations.AnyAsync(x => x.Id == locationId, cancellationToken))
        {
            throw new KeyNotFoundException("Location not found.");
        }

        if (instructorId.HasValue && !await _dbContext.Instructors.AnyAsync(x => x.Id == instructorId.Value, cancellationToken))
        {
            throw new KeyNotFoundException("Instructor not found.");
        }

        if (recurrenceRuleId.HasValue && !await _dbContext.LessonRecurrenceRules.AnyAsync(x => x.Id == recurrenceRuleId.Value, cancellationToken))
        {
            throw new KeyNotFoundException("Recurrence rule not found.");
        }
    }

    private static LessonRecurrenceRule MapToEntity(CreateLessonRecurrenceRuleRequestDto request)
    {
        if (!Enum.TryParse<RecurrenceType>(request.RecurrenceType.Trim(), true, out var recurrenceType))
        {
            throw new InvalidOperationException("RecurrenceType must be Daily, Weekly, or Monthly.");
        }

        if (request.Interval < 1)
        {
            throw new InvalidOperationException("Interval must be at least 1.");
        }

        if (!request.EndDateUtc.HasValue && !request.OccurrenceCount.HasValue)
        {
            throw new InvalidOperationException("Provide EndDateUtc and/or OccurrenceCount for recurrence.");
        }

        return new LessonRecurrenceRule
        {
            Id = Guid.NewGuid(),
            RecurrenceType = recurrenceType,
            Interval = request.Interval,
            DaysOfWeekCsv = string.IsNullOrWhiteSpace(request.DaysOfWeekCsv) ? null : request.DaysOfWeekCsv.Trim(),
            EndDateUtc = request.EndDateUtc,
            OccurrenceCount = request.OccurrenceCount,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static IEnumerable<DateTime> ExpandOccurrences(
        DateTime startUtc,
        RecurrenceType type,
        int interval,
        string? daysOfWeekCsv,
        DateTime? endDateUtc,
        int maxOccurrences)
    {
        var hardEnd = endDateUtc ?? DateTime.UtcNow.AddYears(2);
        var count = 0;

        switch (type)
        {
            case RecurrenceType.Daily:
                for (var t = startUtc; count < maxOccurrences && t <= hardEnd; t = t.AddDays(interval))
                {
                    yield return t;
                    count++;
                }

                break;

            case RecurrenceType.Weekly:
                foreach (var t in ExpandWeekly(startUtc, interval, daysOfWeekCsv, hardEnd, maxOccurrences))
                {
                    yield return t;
                }

                break;

            case RecurrenceType.Monthly:
                for (var i = 0; count < maxOccurrences; i++)
                {
                    var t = AddMonthsClamped(startUtc, i * interval);
                    if (t > hardEnd)
                    {
                        break;
                    }

                    yield return t;
                    count++;
                }

                break;

            default:
                throw new InvalidOperationException("Unsupported recurrence type.");
        }
    }

    private static IEnumerable<DateTime> ExpandWeekly(
        DateTime startUtc,
        int intervalWeeks,
        string? daysCsv,
        DateTime hardEnd,
        int maxCount)
    {
        var time = startUtc.TimeOfDay;
        var days = ParseIsoWeekdays(daysCsv, startUtc);

        var monday = GetMondayOfWeek(startUtc.Date);
        var weekIndex = 0;
        var emitted = 0;

        while (emitted < maxCount)
        {
            var weekMonday = monday.AddDays(7 * intervalWeeks * weekIndex);
            foreach (var isoDow in days)
            {
                var date = weekMonday.AddDays(isoDow - 1);
                var candidate = date + time;
                if (candidate < startUtc || candidate > hardEnd)
                {
                    continue;
                }

                yield return candidate;
                emitted++;
                if (emitted >= maxCount)
                {
                    yield break;
                }
            }

            weekIndex++;
            if (weekIndex > 520)
            {
                yield break;
            }
        }
    }

    /// <summary>ISO weekdays 1=Monday … 7=Sunday. If csv empty, uses weekday of anchor.</summary>
    private static List<int> ParseIsoWeekdays(string? daysCsv, DateTime anchorUtc)
    {
        if (string.IsNullOrWhiteSpace(daysCsv))
        {
            return new List<int> { ToIsoWeekday(anchorUtc) };
        }

        var parts = daysCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var set = new SortedSet<int>();
        foreach (var p in parts)
        {
            if (!int.TryParse(p, out var d) || d < 1 || d > 7)
            {
                throw new InvalidOperationException($"Invalid weekday in DaysOfWeekCsv: '{p}'. Use 1=Monday … 7=Sunday.");
            }

            set.Add(d);
        }

        return set.ToList();
    }

    private static int ToIsoWeekday(DateTime dt)
    {
        return dt.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dt.DayOfWeek;
    }

    private static DateTime GetMondayOfWeek(DateTime date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-offset);
    }

    private static DateTime AddMonthsClamped(DateTime utc, int months)
    {
        var d = utc.Date;
        var target = d.AddMonths(months);
        var day = Math.Min(utc.Day, DateTime.DaysInMonth(target.Year, target.Month));
        return new DateTime(target.Year, target.Month, day, utc.Hour, utc.Minute, utc.Second, DateTimeKind.Utc);
    }

    private static MobileLessonSummaryDto MapToMobileSummaryDto(Lesson x) => new()
    {
        Id = x.Id,
        WorkoutId = x.WorkoutId,
        WorkoutName = x.Workout.Name,
        LocationId = x.LocationId,
        LocationName = x.Location.Name,
        InstructorId = x.InstructorId,
        InstructorName = x.IsInstructorTbd ? "NNB" : x.Instructor?.FullName,
        StartTimeUtc = x.StartTimeUtc,
        DurationMinutes = x.DurationMinutes,
        CapacityOverride = x.CapacityOverride,
        RegisteredCount = x.Reservations.Count(r => r.Status != LessonReservationStatus.Cancelled),
        IsInstructorTbd = x.IsInstructorTbd,
        IsCancelled = x.IsCancelled,
        CancellationReason = x.CancellationReason
    };

    private static LessonDto MapToDto(Lesson x) => new()
    {
        Id = x.Id,
        WorkoutId = x.WorkoutId,
        LocationId = x.LocationId,
        InstructorId = x.InstructorId,
        RecurrenceRuleId = x.RecurrenceRuleId,
        StartTimeUtc = x.StartTimeUtc,
        DurationMinutes = x.DurationMinutes,
        CapacityOverride = x.CapacityOverride,
        IsInstructorTbd = x.IsInstructorTbd,
        IsCancelled = x.IsCancelled,
        CancellationReason = x.CancellationReason
    };

    private static LessonRecurrenceRuleDto MapRuleToDto(LessonRecurrenceRule r) => new()
    {
        Id = r.Id,
        RecurrenceType = r.RecurrenceType.ToString(),
        Interval = r.Interval,
        DaysOfWeekCsv = r.DaysOfWeekCsv,
        EndDateUtc = r.EndDateUtc,
        OccurrenceCount = r.OccurrenceCount,
        CreatedAtUtc = r.CreatedAtUtc
    };

    private static string BuildMemberDisplayName(Member member)
    {
        if (!string.IsNullOrWhiteSpace(member.Username))
        {
            return member.Username;
        }

        return string.Join(" ", new[] { member.FirstName, member.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
