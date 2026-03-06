using Academic.Application.Common;
using Academic.Application.Contracts.Certificates;
using Academic.Application.Contracts.Common;

namespace Academic.Application.Abstractions;

public interface ICertificateService
{
    Task<Result<CertificateCreatedDto>> CreateAsync(Guid studentId, CreateCertificateDto request, CancellationToken cancellationToken);
    Task<Result<CertificateDto>> GenerateAsync(Guid actorStudentId, Guid certificateId, GenerateCertificateDto request, CancellationToken cancellationToken);
    Task<Result<FilePayloadDto>> DownloadAsync(Guid actorStudentId, Guid certificateId, CancellationToken cancellationToken);
    Task<Result<CertificateVerificationDto>> VerifyAsync(string verificationCode, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<CertificateSummaryDto>>> GetMyCertificatesAsync(Guid studentId, CancellationToken cancellationToken);
    Task<Result<CertificateCancellationDto>> CancelAsync(Guid studentId, Guid certificateId, CancellationToken cancellationToken);
}
