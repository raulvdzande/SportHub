namespace SportHub.API.Domain.Entities;

public class Location
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public bool IsSpinningRoom { get; set; }
    public int? SpinningBikeCount { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

