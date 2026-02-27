using Academic.Application.Features.Courses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academic.Api.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
[Route("courses")]
public sealed class CoursesController(IMediator mediator) : ControllerBase
{
    [HttpGet("pensum")]
    public async Task<ActionResult> Pensum(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPensumQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("overdue")]
    public async Task<ActionResult> Overdue(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOverdueCoursesQuery(), cancellationToken);
        return this.ToActionResult(result);
    }
}
