using Academic.Application.Features.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academic.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("reports")]
public sealed class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("transfers")]
    public async Task<ActionResult> Transfers(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTransfersReportQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("enrollments")]
    public async Task<ActionResult> Enrollments(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEnrollmentsReportQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("certificates")]
    public async Task<ActionResult> Certificates(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCertificatesReportQuery(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{reportType}/export")]
    public async Task<ActionResult> Export([FromRoute] string reportType, [FromQuery] string format, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ExportReportQuery(reportType, format), cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }
}
