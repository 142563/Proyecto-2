using Academic.Application.Contracts.Certificates;
using Academic.Application.Features.Certificates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academic.Api.Controllers;

[ApiController]
[Route("certificates")]
public sealed class CertificatesController(IMediator mediator) : ControllerBase
{
    [Authorize(Roles = "Student")]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateCertificateDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateCertificateCommand(request), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = "Student")]
    [HttpPost("{id:guid}/generate")]
    public async Task<ActionResult> Generate([FromRoute] Guid id, [FromBody] GenerateCertificateDto request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GenerateCertificateCommand(id, request), cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize(Roles = "Student")]
    [HttpGet("{id:guid}/download")]
    public async Task<ActionResult> Download([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DownloadCertificateQuery(id), cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    [AllowAnonymous]
    [HttpGet("verify/{code}")]
    public async Task<ActionResult> Verify([FromRoute] string code, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new VerifyCertificateQuery(code), cancellationToken);
        return this.ToActionResult(result);
    }
}
