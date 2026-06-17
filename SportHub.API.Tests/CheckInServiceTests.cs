using System;
using Xunit;
using SportHub.API.Domain.Entities;
using SportHub.API.Domain.Enums;

namespace SportHub.API.Tests;

public class CheckInEntityTests
{
    [Fact]
    public void CheckIn_Constructor_CreatesValidCheckInRecord()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var method = CheckInMethod.Manual;

        // Act
        var checkIn = new CheckIn
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            LessonId = lessonId,
            Method = method,
            CheckedInAtUtc = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, checkIn.Id);
        Assert.Equal(memberId, checkIn.MemberId);
        Assert.Equal(lessonId, checkIn.LessonId);
        Assert.Equal(method, checkIn.Method);
        Assert.True(checkIn.CheckedInAtUtc > DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void CheckIn_RfidMethod_StoresDeviceIdentifier()
    {
        // Arrange
        var checkIn = new CheckIn
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            LessonId = Guid.NewGuid(),
            Method = CheckInMethod.Rfid,
            DeviceIdentifier = "RFID-12345678",
            CheckedInAtUtc = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Equal(CheckInMethod.Rfid, checkIn.Method);
        Assert.NotNull(checkIn.DeviceIdentifier);
        Assert.Equal("RFID-12345678", checkIn.DeviceIdentifier);
    }

    [Fact]
    public void CheckIn_GpsMethod_StoresCoordinates()
    {
        // Arrange
        var checkIn = new CheckIn
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            LessonId = Guid.NewGuid(),
            Method = CheckInMethod.Gps,
            Latitude = 52.3676,
            Longitude = 4.9041,
            CheckedInAtUtc = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Equal(CheckInMethod.Gps, checkIn.Method);
        Assert.NotNull(checkIn.Latitude);
        Assert.NotNull(checkIn.Longitude);
        Assert.True(Math.Abs(checkIn.Latitude.Value - 52.3676) < 0.0001);
        Assert.True(Math.Abs(checkIn.Longitude.Value - 4.9041) < 0.0001);
    }
}
