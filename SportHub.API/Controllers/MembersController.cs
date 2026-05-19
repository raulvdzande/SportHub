using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Members;
using SportHub.API.Application.Interfaces;

namespace SportHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MemberDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _memberService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MemberDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _memberService.GetByIdAsync(id, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create([FromBody] CreateMemberRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _memberService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MemberDto>> Update(Guid id, [FromBody] UpdateMemberRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _memberService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _memberService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
