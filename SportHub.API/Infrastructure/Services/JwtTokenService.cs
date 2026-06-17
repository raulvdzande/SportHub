using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using SportHub.API.Application.Interfaces;
using SportHub.API.Configuration;
using SportHub.API.Domain.Entities;

namespace SportHub.API.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public (string AccessToken, DateTime ExpiresAtUtc) GenerateToken(StaffUser staffUser)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, staffUser.Id.ToString()),
            new Claim(ClaimTypes.Name, staffUser.DisplayName),
            new Claim(ClaimTypes.Email, staffUser.Email),
            new Claim(ClaimTypes.Role, "Employee")
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return (token, expiresAt);
    }

    public (string AccessToken, DateTime ExpiresAtUtc) GenerateTokenForMember(Member member)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
            new Claim(ClaimTypes.Name, member.FirstName + " " + member.LastName),
            new Claim(ClaimTypes.Email, member.Email),
            new Claim("Member", "true")
        };

        // If member has a username we include it as a claim
        if (!string.IsNullOrWhiteSpace(member.Username))
        {
            claims.Add(new Claim(ClaimTypes.Name, member.Username));
        }

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return (token, expiresAt);
    }

    public (string AccessToken, DateTime ExpiresAtUtc) GenerateTokenForInstructor(Instructor instructor)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, instructor.Id.ToString()),
            new Claim(ClaimTypes.Name, instructor.FullName),
            new Claim("role", "Instructor")
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return (token, expiresAt);
    }
}