using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportHub.Shared.DTOs.Reservations;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;
using SportHub.API.Infrastructure.Data.DbContext;

namespace SportHub.API.Application.Services;

public class LessonReservationService : ILessonReservationService
{
    private const int MaxDaysInAdvance = 7;

    private readonly AppDbContext _dbContext;
    private readonly ILogger<LessonReservationService> _logger;

    public LessonReservationService(AppDbContext dbContext, ILogger<LessonReservationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<LessonReservationDto> CreateAsync(Guid memberId, CreateReservationRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate member exists
            var member = await _dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == memberId, cancellationToken)
                ?? throw new KeyNotFoundException("Member not found.");

            // Validate lesson exists and load details
            var lesson = await _dbContext.Lessons
                .Include(x => x.Workout)
                .Include(x => x.Location)
                .Include(x => x.Instructor)
                .Include(x => x.Reservations)
                .FirstOrDefaultAsync(x => x.Id == request.LessonId, cancellationToken)
                ?? throw new KeyNotFoundException("Lesson not found.");

            // Check if lesson is already cancelled
            if (lesson.IsCancelled)
            {
                throw new InvalidOperationException("Cannot reserve a cancelled lesson.");
            }

            // Check advance booking limit (7 days)
            var daysUntilLesson = (lesson.StartTimeUtc - DateTime.UtcNow).TotalDays;
            if (daysUntilLesson > MaxDaysInAdvance)
            {
                throw new InvalidOperationException($"Lessons can only be reserved up to {MaxDaysInAdvance} days in advance.");
            }

            // Check if member already has a reservation (not cancelled) for this lesson
            var existingReservation = await _dbContext.LessonReservations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MemberId == memberId &&
                                          x.LessonId == request.LessonId &&
                                          x.Status != LessonReservationStatus.Cancelled, cancellationToken);

            if (existingReservation != null)
            {
                throw new InvalidOperationException("Member already has an active reservation for this lesson.");
            }

            // Get member's active subscription
            var activeSubscription = await _dbContext.MemberSubscriptions
                .Include(x => x.Plan)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MemberId == memberId &&
                                          x.Status == MembershipStatus.Active &&
                                          x.StartsAtUtc <= DateTime.UtcNow &&
                                          x.EndsAtUtc >= DateTime.UtcNow, cancellationToken);

            if (activeSubscription == null)
            {
                throw new InvalidOperationException("Member does not have an active subscription.");
            }

            // Check session limit for the week if the plan has a limit
            if (activeSubscription.Plan.SessionsPerWeekLimit.HasValue)
            {
                var weekStart = GetWeekStart(lesson.StartTimeUtc);
                var weekEnd = weekStart.AddDays(7);

                var sessionsThisWeek = await _dbContext.LessonReservations
                    .AsNoTracking()
                    .Where(x => x.MemberId == memberId &&
                               x.Status == LessonReservationStatus.Reserved &&
                               x.Lesson.StartTimeUtc >= weekStart &&
                               x.Lesson.StartTimeUtc < weekEnd)
                    .Include(x => x.Lesson)
                    .CountAsync(cancellationToken);

                if (sessionsThisWeek >= activeSubscription.Plan.SessionsPerWeekLimit.Value)
                {
                    throw new InvalidOperationException(
                        $"Member has reached the weekly session limit ({activeSubscription.Plan.SessionsPerWeekLimit}) for this subscription.");
                }
            }

            // Determine the capacity of the lesson
            var capacity = lesson.CapacityOverride ?? lesson.Location.Capacity;

            // Count current reserved (not cancelled) reservations
            var reservedCount = lesson.Reservations.Count(x => x.Status == LessonReservationStatus.Reserved);

            // Create the reservation
            var reservation = new LessonReservation
            {
                Id = Guid.NewGuid(),
                MemberId = memberId,
                LessonId = request.LessonId,
                SpinningBikeNumber = request.SpinningBikeNumber,
                CreatedAtUtc = DateTime.UtcNow
            };

            if (reservedCount < capacity)
            {
                // Space available - add as Reserved
                reservation.Status = LessonReservationStatus.Reserved;
                reservation.WaitlistPosition = null;
            }
            else
            {
                // No space - add to waitlist
                reservation.Status = LessonReservationStatus.Waitlisted;
                var maxWaitlistPosition = lesson.Reservations
                    .Where(x => x.Status == LessonReservationStatus.Waitlisted)
                    .Max(x => x.WaitlistPosition) ?? 0;
                reservation.WaitlistPosition = maxWaitlistPosition + 1;
            }

            _dbContext.LessonReservations.Add(reservation);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reservation created for member {MemberId} on lesson {LessonId} with status {Status}.",
                memberId, request.LessonId, reservation.Status);

            return MapToDto(reservation, lesson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation for member {MemberId} on lesson {LessonId}.",
                memberId, request.LessonId);
            throw;
        }
    }

    public async Task<LessonReservationDto> CancelAsync(Guid reservationId, Guid memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reservation = await _dbContext.LessonReservations
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Workout)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Location)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Instructor)
                .FirstOrDefaultAsync(x => x.Id == reservationId && x.MemberId == memberId, cancellationToken)
                ?? throw new KeyNotFoundException("Reservation not found.");

            // Only allow cancellation if status is Reserved or Waitlisted
            if (reservation.Status != LessonReservationStatus.Reserved &&
                reservation.Status != LessonReservationStatus.Waitlisted)
            {
                throw new InvalidOperationException(
                    $"Cannot cancel a reservation with status {reservation.Status}. Only Reserved or Waitlisted reservations can be cancelled.");
            }

            var wasReserved = reservation.Status == LessonReservationStatus.Reserved;
            var wasWaitlisted = reservation.Status == LessonReservationStatus.Waitlisted;

            // Mark as cancelled
            reservation.Status = LessonReservationStatus.Cancelled;
            reservation.CancelledAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // If was reserved and there are waitlisted members, promote the first one
            if (wasReserved)
            {
                var nextWaitlisted = await _dbContext.LessonReservations
                    .Where(x => x.LessonId == reservation.LessonId &&
                                x.Status == LessonReservationStatus.Waitlisted)
                    .OrderBy(x => x.WaitlistPosition)
                    .FirstOrDefaultAsync(cancellationToken);

                if (nextWaitlisted != null)
                {
                    nextWaitlisted.Status = LessonReservationStatus.Reserved;
                    nextWaitlisted.WaitlistPosition = null;

                    // Update all subsequent waitlist positions
                    var remainingWaitlisted = await _dbContext.LessonReservations
                        .Where(x => x.LessonId == reservation.LessonId &&
                                   x.Status == LessonReservationStatus.Waitlisted &&
                                   x.Id != nextWaitlisted.Id)
                        .OrderBy(x => x.WaitlistPosition)
                        .ToListAsync(cancellationToken);

                    for (int i = 0; i < remainingWaitlisted.Count; i++)
                    {
                        remainingWaitlisted[i].WaitlistPosition = i + 1;
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Promoted waitlisted member {PromotedMemberId} from waitlist for lesson {LessonId}.",
                        nextWaitlisted.MemberId, reservation.LessonId);
                }
            }
            else if (wasWaitlisted)
            {
                // Update waitlist positions for remaining waitlisted members
                var remainingWaitlisted = await _dbContext.LessonReservations
                    .Where(x => x.LessonId == reservation.LessonId &&
                               x.Status == LessonReservationStatus.Waitlisted &&
                               x.Id != reservationId)
                    .OrderBy(x => x.WaitlistPosition)
                    .ToListAsync(cancellationToken);

                for (int i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Reservation {ReservationId} cancelled for member {MemberId}.",
                reservationId, memberId);

            return MapToDto(reservation, reservation.Lesson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation {ReservationId} for member {MemberId}.",
                reservationId, memberId);
            throw;
        }
    }

    public async Task<Guid?> RemoveParticipantAsync(Guid lessonId, Guid memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reservation = await _dbContext.LessonReservations
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Workout)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Location)
                .FirstOrDefaultAsync(x => x.LessonId == lessonId && x.MemberId == memberId, cancellationToken)
                ?? throw new KeyNotFoundException("Reservation not found.");

            var wasReserved = reservation.Status == LessonReservationStatus.Reserved;

            // Mark as cancelled
            reservation.Status = LessonReservationStatus.Cancelled;
            reservation.CancelledAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // If was reserved and there are waitlisted members, send them a notification asking if they want the spot
            Guid? notificationId = null;
            if (wasReserved)
            {
                var nextWaitlisted = await _dbContext.LessonReservations
                    .Include(x => x.Member)
                    .Where(x => x.LessonId == lessonId &&
                                x.Status == LessonReservationStatus.Waitlisted)
                    .OrderBy(x => x.WaitlistPosition)
                    .FirstOrDefaultAsync(cancellationToken);

                if (nextWaitlisted != null)
                {
                    // Send notification asking if they want to accept the spot
                    notificationId = Guid.NewGuid();
                    var notification = new Notification
                    {
                        Id = notificationId.Value,
                        MemberId = nextWaitlisted.MemberId,
                        LessonId = lessonId,
                        Title = "Spot Available",
                        Message = $"A spot opened for {reservation.Lesson.Workout.Name} at {reservation.Lesson.StartTimeUtc:HH:mm}. Accept to join?",
                        Type = NotificationType.WaitlistSpotOpened,
                        Status = NotificationStatus.Pending,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    _logger.LogInformation("CREATING NOTIFICATION: Id={NotificationId}, MemberId={MemberId}, LessonId={LessonId}, Type={Type}, Status={Status}",
                        notification.Id, notification.MemberId, notification.LessonId, notification.Type, notification.Status);

                    _dbContext.Notifications.Add(notification);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("NOTIFICATION SAVED: {NotificationId} for member {MemberId}", notification.Id, nextWaitlisted.MemberId);
                }
            }

            _logger.LogInformation("Participant {MemberId} removed from lesson {LessonId}.",
                memberId, lessonId);

            return notificationId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant {MemberId} from lesson {LessonId}.",
                memberId, lessonId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<LessonReservationDto>> GetByMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reservations = await _dbContext.LessonReservations
                .AsNoTracking()
                .Where(x => x.MemberId == memberId && x.Status != LessonReservationStatus.Cancelled)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Workout)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Location)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Instructor)
                .OrderBy(x => x.Lesson.StartTimeUtc)
                .ToListAsync(cancellationToken);

            return reservations.ConvertAll(x => MapToDto(x, x.Lesson));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations for member {MemberId}.", memberId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<LessonReservationDto>> GetByLessonAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reservations = await _dbContext.LessonReservations
                .AsNoTracking()
                .Where(x => x.LessonId == lessonId && x.Status != LessonReservationStatus.Cancelled)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Workout)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Location)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Instructor)
                .OrderBy(x => x.Status)
                    .ThenBy(x => x.WaitlistPosition)
                    .ThenBy(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            return reservations.ConvertAll(x => MapToDto(x, x.Lesson));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations for lesson {LessonId}.", lessonId);
            throw;
        }
    }

    public async Task<LessonReservationDto?> GetMemberReservationForLessonAsync(Guid memberId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reservation = await _dbContext.LessonReservations
                .AsNoTracking()
                .Where(x => x.MemberId == memberId &&
                           x.LessonId == lessonId &&
                           x.Status != LessonReservationStatus.Cancelled)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Workout)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Location)
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Instructor)
                .FirstOrDefaultAsync(cancellationToken);

            return reservation != null ? MapToDto(reservation, reservation.Lesson) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation for member {MemberId} on lesson {LessonId}.",
                memberId, lessonId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<int>> GetTakenBikesAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        try
        {
            var takenBikes = await _dbContext.LessonReservations
                .AsNoTracking()
                .Where(x => x.LessonId == lessonId &&
                           x.Status == LessonReservationStatus.Reserved &&
                           x.SpinningBikeNumber.HasValue)
                .Select(x => x.SpinningBikeNumber!.Value)
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            return takenBikes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving taken bikes for lesson {LessonId}.", lessonId);
            throw;
        }
    }

    private static DateTime GetWeekStart(DateTime dateTime)
    {
        var date = dateTime.Date;
        var daysToSubtract = ((int)date.DayOfWeek + 6) % 7; // Monday is 0
        return date.AddDays(-daysToSubtract);
    }

    private static LessonReservationDto MapToDto(LessonReservation reservation, Lesson lesson)
    {
        return new LessonReservationDto
        {
            Id = reservation.Id,
            MemberId = reservation.MemberId,
            LessonId = reservation.LessonId,
            Status = reservation.Status.ToString(),
            CreatedAtUtc = reservation.CreatedAtUtc,
            CancelledAtUtc = reservation.CancelledAtUtc,
            WaitlistPosition = reservation.WaitlistPosition,
            SpinningBikeNumber = reservation.SpinningBikeNumber,
            LessonStartTimeUtc = lesson.StartTimeUtc,
            LessonDurationMinutes = lesson.DurationMinutes,
            WorkoutName = lesson.Workout.Name,
            LocationName = lesson.Location.Name,
            InstructorName = lesson.IsInstructorTbd ? "NNB" : lesson.Instructor?.FullName
        };
    }
}
