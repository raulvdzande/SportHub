using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportHub.Shared.DTOs.Payments;
using SportHub.API.Application.Interfaces;

namespace SportHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IStripePaymentService _stripePayments;

    public PaymentsController(IStripePaymentService stripePayments)
    {
        _stripePayments = stripePayments;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PaymentDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _stripePayments.GetAllAsync(cancellationToken));
    }
}
