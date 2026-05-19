using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SportHub.API.Controllers.Requests;

public class CreateInstructorRequest
{
    [Required]
    [StringLength(160)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [MinLength(6)]
    [StringLength(100)]
    public string? Password { get; set; }

    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    [StringLength(1024)]
    public string? PhotoUrl { get; set; }

    public IFormFile? Photo { get; set; }
    public bool IsTbd { get; set; }
    public bool IsActive { get; set; } = true;
}
