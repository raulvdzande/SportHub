using System.Collections.Concurrent;
using SportHub.API.Application.Interfaces;
using SportHub.API.Domain.Entities;

namespace SportHub.API.Infrastructure.Services;

public class InMemoryStaffTokenStore : IStaffTokenStore
{
    private readonly ConcurrentDictionary<string, StoredToken> _tokens = new();

    public void StoreToken(string token, StaffUser user, DateTime expiresAtUtc)
    {
        _tokens[token] = new StoredToken
        {
            ExpiresAtUtc = expiresAtUtc,
            User = new StaffUser
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive
            }
        };
    }

    public bool TryGetUser(string token, out StaffUser user)
    {
        user = null!;

        if (!_tokens.TryGetValue(token, out var storedToken))
        {
            return false;
        }

        if (storedToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            _tokens.TryRemove(token, out _);
            return false;
        }

        user = storedToken.User;
        return true;
    }

    private class StoredToken
    {
        public DateTime ExpiresAtUtc { get; set; }
        public StaffUser User { get; set; } = null!;
    }
}
