using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SportHub.API.Controllers.Requests;

public class CreateInstructorRequest
{
    [Required]
    [StringLength(160)]
    public string FullName { get; set; } = string.Empty;

    public IFormFile? Photo { get; set; }
    public bool IsTbd { get; set; }
    public bool IsActive { get; set; } = true;
}
