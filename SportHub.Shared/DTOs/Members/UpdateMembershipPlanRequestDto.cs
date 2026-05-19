using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Members;

public class UpdateMembershipPlanRequestDto
{
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string PeriodType { get; set; } = string.Empty;

    public int? SessionsPerWeekLimit { get; set; }

    [Range(typeof(decimal), "0", "999999")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "EUR";

    public bool IsActive { get; set; }
}

