using Academic.Application.Abstractions;
using Academic.Application.Common;
using Academic.Application.Contracts.Common;
using Academic.Application.Contracts.Reports;
using MediatR;

namespace Academic.Application.Features.Reports;

public sealed record GetTransfersReportQuery : IRequest<Result<IReadOnlyList<TransferReportRowDto>>>;

public sealed class GetTransfersReportQueryHandler(IReportService reportService)
    : IRequestHandler<GetTransfersReportQuery, Result<IReadOnlyList<TransferReportRowDto>>>
{
    public Task<Result<IReadOnlyList<TransferReportRowDto>>> Handle(GetTransfersReportQuery request, CancellationToken cancellationToken)
        => reportService.GetTransfersReportAsync(cancellationToken);
}

public sealed record GetEnrollmentsReportQuery : IRequest<Result<IReadOnlyList<EnrollmentReportRowDto>>>;

public sealed class GetEnrollmentsReportQueryHandler(IReportService reportService)
    : IRequestHandler<GetEnrollmentsReportQuery, Result<IReadOnlyList<EnrollmentReportRowDto>>>
{
    public Task<Result<IReadOnlyList<EnrollmentReportRowDto>>> Handle(GetEnrollmentsReportQuery request, CancellationToken cancellationToken)
        => reportService.GetEnrollmentsReportAsync(cancellationToken);
}

public sealed record GetCertificatesReportQuery : IRequest<Result<IReadOnlyList<CertificateReportRowDto>>>;

public sealed class GetCertificatesReportQueryHandler(IReportService reportService)
    : IRequestHandler<GetCertificatesReportQuery, Result<IReadOnlyList<CertificateReportRowDto>>>
{
    public Task<Result<IReadOnlyList<CertificateReportRowDto>>> Handle(GetCertificatesReportQuery request, CancellationToken cancellationToken)
        => reportService.GetCertificatesReportAsync(cancellationToken);
}

public sealed record ExportReportQuery(string ReportType, string Format) : IRequest<Result<FilePayloadDto>>;

public sealed class ExportReportQueryHandler(IReportService reportService)
    : IRequestHandler<ExportReportQuery, Result<FilePayloadDto>>
{
    public Task<Result<FilePayloadDto>> Handle(ExportReportQuery request, CancellationToken cancellationToken)
        => reportService.ExportAsync(request.ReportType, request.Format, cancellationToken);
}
