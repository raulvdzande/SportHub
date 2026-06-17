using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.API.Application.Interfaces;
using SportHub.Shared.DTOs.Auth;

namespace SportHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class InstructorAuthController : ControllerBase
{
    private readonly IInstructorAuthService _service;
    private readonly ILogger<InstructorAuthController> _logger;

    public InstructorAuthController(IInstructorAuthService service, ILogger<InstructorAuthController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Login instructor with email/password.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginInstructorResponseDto>> Login(LoginInstructorRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _service.LoginAsync(request, cancellationToken);
        if (result is null)
            return Unauthorized("E-mail of wachtwoord onjuist.");

        return Ok(result);
    }
}
