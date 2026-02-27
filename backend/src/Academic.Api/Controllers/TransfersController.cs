using Academic.Application.Contracts.Transfers;
using Academic.Application.Features.Transfers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academic.Api.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
[Route("transfers")]
public sealed class TransfersController(IMediator mediator) : ControllerBase
{
    [HttpGet("availability")]
    public async Task<ActionResult> GetAvailability([FromQuery] int campusId, [FromQuery] string shift, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTransferAvailabilityQuery(campusId, shift), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateTransferDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateTransferCommand(request), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("my")]
    public async Task<ActionResult> My(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyTransfersQuery(), cancellationToken);
        return this.ToActionResult(result);
    }
}
