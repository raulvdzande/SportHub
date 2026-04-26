namespace SportHub.API.Configuration;

public class SeedStaffUserOptions
{
    public const string SectionName = "SeedStaffUser";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = "Medewerker";
}

