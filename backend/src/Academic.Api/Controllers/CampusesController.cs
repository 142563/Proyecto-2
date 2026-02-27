using Academic.Application.Features.Campuses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academic.Api.Controllers;

[ApiController]
[Authorize]
[Route("campuses")]
public sealed class CampusesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCampusesQuery(), cancellationToken);
        return this.ToActionResult(result);
    }
}
