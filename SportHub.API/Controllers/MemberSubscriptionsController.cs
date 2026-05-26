using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Members;
using SportHub.API.Application.Interfaces;
using SportHub.API.Controllers.Requests;

namespace SportHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MemberSubscriptionsController : ControllerBase
{
    private readonly IMemberSubscriptionService _subscriptionService;

    public MemberSubscriptionsController(IMemberSubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MemberSubscriptionDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _subscriptionService.GetAllAsync(cancellationToken));
    }

    [HttpGet("member/{memberId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<MemberSubscriptionDto>>> GetByMember(Guid memberId, CancellationToken cancellationToken)
    {
        return Ok(await _subscriptionService.GetByMemberAsync(memberId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<MemberSubscriptionDto>> Create([FromBody] CreateMemberSubscriptionRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _subscriptionService.CreateAsync(request, cancellationToken);
        return Ok(created);
    }

    [HttpPost("upgrade-quote")]
    public async Task<ActionResult<SubscriptionUpgradeQuoteDto>> GetUpgradeQuote(
        [FromBody] CreateSubscriptionUpgradeQuoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.GetUpgradeQuoteAsync(
            request.CurrentSubscriptionId,
            request.TargetPlanId,
            request.CalculatedAtUtc,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<MemberSubscriptionDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _subscriptionService.CancelAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/enable-autorenew")]
    public async Task<ActionResult<MemberSubscriptionDto>> EnableAutoRenew(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _subscriptionService.EnableAutoRenewAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/disable-autorenew")]
    public async Task<ActionResult<MemberSubscriptionDto>> DisableAutoRenew(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _subscriptionService.DisableAutoRenewAsync(id, cancellationToken));
    }
}