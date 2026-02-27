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
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateEnrollmentDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateEnrollmentCommand(request), cancellationToken);
        return this.ToActionResult(result);
    }
}
