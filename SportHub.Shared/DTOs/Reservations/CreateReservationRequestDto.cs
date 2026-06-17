using System.ComponentModel.DataAnnotations;

namespace SportHub.Shared.DTOs.Reservations;

public class CreateReservationRequestDto
{
    [Required]
    public Guid LessonId { get; set; }

    /// <summary>Only set for spinning lessons.</summary>
    public int? SpinningBikeNumber { get; set; }
}
