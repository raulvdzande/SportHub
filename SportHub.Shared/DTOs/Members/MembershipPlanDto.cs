namespace SportHub.Shared.DTOs.Members;

public class MembershipPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PeriodType { get; set; } = string.Empty;
    public int? SessionsPerWeekLimit { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; }
}

