using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.CheckIns;

public class CreateCheckInRequestDto
{
    [Required]
    public Guid LessonId { get; set; }

    /// <summary>Rfid | Gps | Manual</summary>
    [Required]
    public string Method { get; set; } = "Manual";

    /// <summary>RFID tag identifier.</summary>
    public string? DeviceIdentifier { get; set; }

    /// <summary>GPS coordinates for location-based check-in.</summary>
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
