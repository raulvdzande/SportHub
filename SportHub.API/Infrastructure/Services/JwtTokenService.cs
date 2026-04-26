using Microsoft.Extensions.Options;
using SportHub.API.Application.Interfaces;
using SportHub.API.Configuration;
using SportHub.API.Domain.Entities;

namespace SportHub.API.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IStaffTokenStore _tokenStore;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions, IStaffTokenStore tokenStore)
    {
        _jwtOptions = jwtOptions.Value;
        _tokenStore = tokenStore;
    }

    public (string AccessToken, DateTime ExpiresAtUtc) GenerateToken(StaffUser staffUser)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        _tokenStore.StoreToken(token, staffUser, expiresAt);

        return (token, expiresAt);
    }
}
