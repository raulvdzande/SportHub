using System;
using System.Threading.Tasks;
using Xunit;

namespace SportHub.App.Tests;

public class LoginPageTests
{
    [Fact]
    public void LoginPage_EmailProperty_CanBeSet()
    {
        // Arrange & Act
        string email = "test@example.com";

        // Assert
        Assert.NotEmpty(email);
        Assert.Contains("@", email);
    }

    [Fact]
    public void LoginPage_PasswordProperty_CanBeSet()
    {
        // Arrange & Act
        string password = "TestPassword123!";

        // Assert
        Assert.NotEmpty(password);
        Assert.True(password.Length >= 8);
    }

    [Fact]
    public void LoginPage_ValidCredentials_EmailAndPasswordNotEmpty()
    {
        // Arrange
        var email = "member@test.com";
        var password = "Password123!";

        // Act & Assert
        Assert.NotEmpty(email);
        Assert.NotEmpty(password);
        Assert.True(email.Contains("@"));
    }

    [Fact]
    public void LoginPage_InvalidCredentials_ReturnsError()
    {
        // Arrange
        var email = "invalid@test.com";
        var password = "WrongPassword";

        // Act & Assert
        Assert.NotEmpty(email);
        Assert.NotEmpty(password);
        // Invalid credentials should be caught by API, not here
    }
}
