using System;
using Xunit;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;

namespace SportHub.API.Tests;

public class LessonReservationEntityTests
{
    [Fact]
    public void LessonReservation_CreateReserved_SetStatusCorrectly()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var reservation = new LessonReservation
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            LessonId = lessonId,
            Status = LessonReservationStatus.Reserved,
            CreatedAtUtc = now
        };

        // Assert
        Assert.Equal(memberId, reservation.MemberId);
        Assert.Equal(lessonId, reservation.LessonId);
        Assert.Equal(LessonReservationStatus.Reserved, reservation.Status);
        Assert.Null(reservation.WaitlistPosition);
        Assert.Null(reservation.CancelledAtUtc);
    }

    [Fact]
    public void LessonReservation_CreateWaitlisted_SetsWaitlistPosition()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var waitlistPosition = 3;
        var now = DateTime.UtcNow;

        // Act
        var reservation = new LessonReservation
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            LessonId = lessonId,
            Status = LessonReservationStatus.Waitlisted,
            WaitlistPosition = waitlistPosition,
            CreatedAtUtc = now
        };

        // Assert
        Assert.Equal(LessonReservationStatus.Waitlisted, reservation.Status);
        Assert.Equal(waitlistPosition, reservation.WaitlistPosition);
        Assert.Null(reservation.CancelledAtUtc);
    }

    [Fact]
    public void LessonReservation_CancelReservation_SetsCancelledAtUtc()
    {
        // Arrange
        var reservation = new LessonReservation
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            LessonId = Guid.NewGuid(),
            Status = LessonReservationStatus.Reserved,
            CreatedAtUtc = DateTime.UtcNow
        };

        var cancelTime = DateTime.UtcNow;

        // Act
        reservation.Status = LessonReservationStatus.Cancelled;
        reservation.CancelledAtUtc = cancelTime;

        // Assert
        Assert.Equal(LessonReservationStatus.Cancelled, reservation.Status);
        Assert.NotNull(reservation.CancelledAtUtc);
        Assert.True((cancelTime - reservation.CancelledAtUtc.Value).TotalSeconds < 1);
    }
}
