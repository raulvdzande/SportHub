using System.ComponentModel.DataAnnotations;

namespace SportHub.API.Application.DTOs.Locations;

public class UpdateLocationRequestDto
{
    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, 2000)]
    public int Capacity { get; set; }

    public bool IsSpinningRoom { get; set; }

    [Range(1, 300)]
    public int? SpinningBikeCount { get; set; }

    public bool IsActive { get; set; } = true;
}

