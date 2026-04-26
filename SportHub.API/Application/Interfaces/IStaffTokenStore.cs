using SportHub.API.Domain.Entities;

namespace SportHub.API.Application.Interfaces;

public interface IStaffTokenStore
{
    void StoreToken(string token, StaffUser user, DateTime expiresAtUtc);
    bool TryGetUser(string token, out StaffUser user);
}

