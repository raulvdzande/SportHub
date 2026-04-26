namespace SportHub.API.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "SportHub.API";
    public string Audience { get; set; } = "SportHub.Web";
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 120;
}

