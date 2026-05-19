using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Payments;
using SportHub.API.Application.Interfaces;

namespace SportHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StripeController : ControllerBase
{
    private readonly IStripePaymentService _stripePayments;

    public StripeController(IStripePaymentService stripePayments)
    {
        _stripePayments = stripePayments;
    }

    /// <summary>Create a Stripe PaymentIntent (test mode) and persist a pending payment row.</summary>
    [Authorize]
    [HttpPost("payment-intents")]
    public async Task<ActionResult<CreateStripePaymentIntentResponseDto>> CreatePaymentIntent(
        [FromBody] CreateStripePaymentIntentRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _stripePayments.CreatePaymentIntentAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Developer shortcut: mark the local payment as succeeded without Stripe webhook.</summary>
    [Authorize]
    [HttpPost("simulate-succeeded")]
    public async Task<ActionResult<PaymentDto>> SimulateSucceeded(
        [FromBody] SimulateStripePaymentRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _stripePayments.SimulateSucceededAsync(request.PaymentIntentId.Trim(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Stripe webhook endpoint. Configure URL in Stripe Dashboard with signing secret.</summary>
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrEmpty(signature))
        {
            return BadRequest("Missing Stripe-Signature header.");
        }

        var result = await _stripePayments.HandleWebhookAsync(json, signature, cancellationToken);
        return result is null ? Ok() : Ok(result);
    }
    
    [Authorize]
    [HttpPost("checkout-session")]
    public async Task<ActionResult<CreateCheckoutSessionResponseDto>>
        CreateCheckoutSession(
            [FromBody] CreateStripePaymentIntentRequestDto request,
            CancellationToken cancellationToken)
    {
        var result =
            await _stripePayments.CreateCheckoutSessionAsync(
                request,
                cancellationToken);

        return Ok(result);
    }
}
