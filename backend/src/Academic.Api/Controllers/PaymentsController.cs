using Academic.Application.Features.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academic.Api.Controllers;

[ApiController]
[Authorize]
[Route("payments")]
public sealed class PaymentsController(IMediator mediator) : ControllerBase
{
    [Authorize(Roles = "Student")]
    [HttpGet("my")]
    public async Task<ActionResult> My(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyPaymentsQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/mark-paid")]
    public async Task<ActionResult> MarkPaid([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new MarkPaymentPaidCommand(id), cancellationToken);
        return this.ToActionResult(result);
    }
}
