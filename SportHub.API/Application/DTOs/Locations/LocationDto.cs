namespace SportHub.API.Application.DTOs.Locations;

public class LocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool IsSpinningRoom { get; set; }
    public int? SpinningBikeCount { get; set; }
    public bool IsActive { get; set; }
}

