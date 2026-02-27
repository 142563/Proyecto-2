using Academic.Application.Common;
using Academic.Application.Contracts.Common;
using Academic.Application.Contracts.Reports;

namespace Academic.Application.Abstractions;

public interface IReportService
{
    Task<Result<IReadOnlyList<TransferReportRowDto>>> GetTransfersReportAsync(CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<EnrollmentReportRowDto>>> GetEnrollmentsReportAsync(CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<CertificateReportRowDto>>> GetCertificatesReportAsync(CancellationToken cancellationToken);
    Task<Result<FilePayloadDto>> ExportAsync(string reportType, string format, CancellationToken cancellationToken);
}
