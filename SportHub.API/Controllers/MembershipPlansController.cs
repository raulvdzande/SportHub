using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Members;
using SportHub.API.Application.Interfaces;

namespace SportHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MembershipPlansController : ControllerBase
{
    private readonly IMembershipPlanService _planService;

    public MembershipPlansController(IMembershipPlanService planService)
    {
        _planService = planService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MembershipPlanDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _planService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MembershipPlanDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _planService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<MembershipPlanDto>> Create([FromBody] CreateMembershipPlanRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _planService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MembershipPlanDto>> Update(Guid id, [FromBody] UpdateMembershipPlanRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _planService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _planService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
