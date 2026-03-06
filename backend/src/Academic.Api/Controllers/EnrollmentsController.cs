using Academic.Application.Contracts.Courses;
using Academic.Application.Features.Courses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academic.Api.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
[Route("enrollments")]
public sealed class EnrollmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("my")]
    public async Task<ActionResult> My(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyEnrollmentsQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateEnrollmentDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateEnrollmentCommand(request), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CancelEnrollmentCommand(id), cancellationToken);
        return this.ToActionResult(result);
    }
}
