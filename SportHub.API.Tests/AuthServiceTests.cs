using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using SportHub.API.Domain.Entities;

namespace SportHub.API.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void PasswordHasher_HashPassword_CreatesNonEmptyHash()
    {
        // Arrange
        var hasher = new PasswordHasher<Member>();
        var member = new Member { Id = Guid.NewGuid(), Email = "test@example.com" };
        var password = "TestPassword123!";

        // Act
        var hash = hasher.HashPassword(member, password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash); // Hash should differ from plain password
    }

    [Fact]
    public void PasswordHasher_VerifyHashedPassword_SucceedsWithCorrectPassword()
    {
        // Arrange
        var hasher = new PasswordHasher<Member>();
        var member = new Member { Id = Guid.NewGuid(), Email = "test@example.com" };
        var password = "CorrectPassword123!";
        var hash = hasher.HashPassword(member, password);

        // Act
        var result = hasher.VerifyHashedPassword(member, hash, password);

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void PasswordHasher_VerifyHashedPassword_FailsWithWrongPassword()
    {
        // Arrange
        var hasher = new PasswordHasher<Member>();
        var member = new Member { Id = Guid.NewGuid(), Email = "test@example.com" };
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = hasher.HashPassword(member, correctPassword);

        // Act
        var result = hasher.VerifyHashedPassword(member, hash, wrongPassword);

        // Assert
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }
}
