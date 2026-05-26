using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Members;
using SportHub.API.Application.Interfaces;
using SportHub.API.Controllers.Requests;

namespace SportHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IMemberSubscriptionService _subscriptionService;
    private readonly IPhotoStorageService _photoStorageService;

    public MembersController(
        IMemberService memberService,
        IMemberSubscriptionService subscriptionService,
        IPhotoStorageService photoStorageService)
    {
        _memberService = memberService;
        _subscriptionService = subscriptionService;
        _photoStorageService = photoStorageService;
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

    [HttpGet("me")]
    public async Task<ActionResult<MemberDto>> GetMe(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentMemberId(out var memberId))
        {
            return Unauthorized();
        }

        return Ok(await _memberService.GetByIdAsync(memberId, cancellationToken));
    }

    [HttpGet("me/subscriptions")]
    public async Task<ActionResult<IReadOnlyCollection<MemberSubscriptionDto>>> GetMySubscriptions(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentMemberId(out var memberId))
        {
            return Unauthorized();
        }

        return Ok(await _subscriptionService.GetByMemberAsync(memberId, cancellationToken));
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

    [HttpPut("me")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MemberDto>> UpdateMe([FromForm] UpdateMemberProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentMemberId(out var memberId))
        {
            return Unauthorized();
        }

        var existing = await _memberService.GetByIdAsync(memberId, cancellationToken);

        var photoUrl = request.Photo is not null
            ? await _photoStorageService.SaveMemberPhotoAsync(request.Photo, cancellationToken)
            : existing.ProfilePhotoUrl;

        var updated = await _memberService.UpdateAsync(memberId, new UpdateMemberRequestDto
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Username = request.Username,
            PhoneNumber = request.PhoneNumber,
            ProfilePhotoUrl = photoUrl,
            IsActive = existing.IsActive
        }, cancellationToken);

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _memberService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private bool TryGetCurrentMemberId(out Guid memberId)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out memberId);
    }
}
