using SportHub.API.Domain.Entities;

namespace SportHub.API.Application.Interfaces;

public interface IJwtTokenService
{
    (string AccessToken, DateTime ExpiresAtUtc) GenerateToken(StaffUser staffUser);
}

