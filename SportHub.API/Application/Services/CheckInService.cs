using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportHub.Shared.DTOs.CheckIns;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class CheckInService : ICheckInService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CheckInService> _logger;

    public CheckInService(AppDbContext dbContext, ILogger<CheckInService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CheckInDto> CreateAsync(Guid memberId, CreateCheckInRequestDto request, CancellationToken cancellationToken = default)
    {
        // Validate member and lesson exist
        var memberExists = await _dbContext.Members
            .AnyAsync(x => x.Id == memberId, cancellationToken);

        if (!memberExists)
        {
            _logger.LogWarning("Member {MemberId} not found for check-in", memberId);
            throw new KeyNotFoundException("Member not found.");
        }

        var lesson = await _dbContext.Lessons
            .AsNoTracking()
            .Include(x => x.Workout)
            .Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.LogWarning("Lesson {LessonId} not found for check-in", request.LessonId);
            throw new KeyNotFoundException("Lesson not found.");
        }

        // Validate and parse the method enum
        if (!Enum.TryParse<Domain.Enums.CheckInMethod>(request.Method.Trim(), true, out var method))
        {
            _logger.LogWarning("Invalid check-in method: {Method}", request.Method);
            throw new InvalidOperationException("CheckInMethod must be Rfid, Gps, or Manual.");
        }

        // Create the check-in entity
        var checkIn = new CheckIn
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            LessonId = request.LessonId,
            Method = method,
            CheckedInAtUtc = DateTime.UtcNow,
            DeviceIdentifier = request.DeviceIdentifier?.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        _dbContext.CheckIns.Add(checkIn);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Check-in created: {CheckInId} for Member {MemberId} in Lesson {LessonId}",
            checkIn.Id, memberId, request.LessonId);

        // Return DTO with denormalized lesson info
        return MapToDto(checkIn, lesson, null);
    }

    public async Task<IReadOnlyCollection<CheckInDto>> GetByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        // Verify lesson exists
        var lesson = await _dbContext.Lessons
            .AsNoTracking()
            .Include(x => x.Workout)
            .Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.LogWarning("Lesson {LessonId} not found for check-in query", lessonId);
            throw new KeyNotFoundException("Lesson not found.");
        }

        var checkIns = await _dbContext.CheckIns
            .AsNoTracking()
            .Where(x => x.LessonId == lessonId)
            .Include(x => x.Member)
            .OrderBy(x => x.CheckedInAtUtc)
            .ToListAsync(cancellationToken);

        return checkIns.Select(ci => MapToDto(ci, lesson, ci.Member)).ToList();
    }

    public async Task<IReadOnlyCollection<CheckInDto>> GetMemberHistoryAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        // Verify member exists
        var memberExists = await _dbContext.Members
            .AnyAsync(x => x.Id == memberId, cancellationToken);

        if (!memberExists)
        {
            _logger.LogWarning("Member {MemberId} not found for history query", memberId);
            throw new KeyNotFoundException("Member not found.");
        }

        // Get check-ins from the last 12 months
        var cutoffDate = DateTime.UtcNow.AddMonths(-12);

        var checkIns = await _dbContext.CheckIns
            .AsNoTracking()
            .Where(x => x.MemberId == memberId && x.CheckedInAtUtc >= cutoffDate)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Workout)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Location)
            .OrderByDescending(x => x.CheckedInAtUtc)
            .ToListAsync(cancellationToken);

        return checkIns.Select(ci => MapToDto(ci, ci.Lesson, null)).ToList();
    }

    private static CheckInDto MapToDto(CheckIn checkIn, Lesson lesson, Member? member)
    {
        return new CheckInDto
        {
            Id = checkIn.Id,
            MemberId = checkIn.MemberId,
            LessonId = checkIn.LessonId,
            Method = checkIn.Method.ToString(),
            CheckedInAtUtc = checkIn.CheckedInAtUtc,
            DeviceIdentifier = checkIn.DeviceIdentifier,
            Latitude = checkIn.Latitude,
            Longitude = checkIn.Longitude,
            WorkoutName = lesson.Workout.Name,
            LocationName = lesson.Location.Name,
            LessonStartTimeUtc = lesson.StartTimeUtc,
            DisplayName = member is not null ? BuildMemberDisplayName(member) : string.Empty,
            Username = member?.Username,
            ProfilePhotoUrl = member?.ProfilePhotoUrl
        };
    }

    private static string BuildMemberDisplayName(Member member)
    {
        if (!string.IsNullOrWhiteSpace(member.Username))
        {
            return member.Username;
        }

        return string.Join(" ", new[] { member.FirstName, member.LastName }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
