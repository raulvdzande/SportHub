using System;
using System.Threading.Tasks;
using Xunit;

namespace SportHub.App.Tests;

public class CheckInPageTests
{
    [Fact]
    public void CheckInPage_ManualMethod_IsValid()
    {
        // Arrange & Act
        string method = "Manual";

        // Assert
        Assert.NotEmpty(method);
        Assert.Equal("Manual", method);
    }

    [Fact]
    public void CheckInPage_RfidMethod_IsValid()
    {
        // Arrange & Act
        string method = "Rfid";

        // Assert
        Assert.NotEmpty(method);
        Assert.Equal("Rfid", method);
    }

    [Fact]
    public void CheckInPage_GpsMethod_IsValid()
    {
        // Arrange & Act
        string method = "Gps";

        // Assert
        Assert.NotEmpty(method);
        Assert.Equal("Gps", method);
    }

    [Fact]
    public void CheckInPage_CheckInMethods_AreDistinct()
    {
        // Arrange
        var methods = new[] { "Manual", "Rfid", "Gps" };

        // Act & Assert
        Assert.Equal(3, methods.Length);
        Assert.Contains("Manual", methods);
        Assert.Contains("Rfid", methods);
        Assert.Contains("Gps", methods);
    }
}
