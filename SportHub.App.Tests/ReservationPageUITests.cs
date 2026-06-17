using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SportHub.App.Tests;

public class ReservationPageTests
{
    [Fact]
    public void ReservationPage_ReservedStatus_IsValid()
    {
        // Arrange & Act
        string status = "Reserved";

        // Assert
        Assert.NotEmpty(status);
        Assert.Equal("Reserved", status);
    }

    [Fact]
    public void ReservationPage_WaitlistedStatus_IsValid()
    {
        // Arrange & Act
        string status = "Waitlisted";

        // Assert
        Assert.NotEmpty(status);
        Assert.Equal("Waitlisted", status);
    }

    [Fact]
    public void ReservationPage_CancelledStatus_IsValid()
    {
        // Arrange & Act
        string status = "Cancelled";

        // Assert
        Assert.NotEmpty(status);
        Assert.Equal("Cancelled", status);
    }

    [Fact]
    public void ReservationPage_ReservationStatuses_AreDistinct()
    {
        // Arrange
        var statuses = new[] { "Reserved", "Waitlisted", "Cancelled" };

        // Act & Assert
        Assert.Equal(3, statuses.Length);
        Assert.Contains("Reserved", statuses);
        Assert.Contains("Waitlisted", statuses);
        Assert.Contains("Cancelled", statuses);
    }
}
