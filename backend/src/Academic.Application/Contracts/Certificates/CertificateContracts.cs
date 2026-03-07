namespace Academic.Application.Contracts.Certificates;

public sealed record CreateCertificateDto(string Purpose);

public sealed record GenerateCertificateDto(bool SendEmail, bool IncludeQr);

public sealed record CertificateTypeDto(
    string Code,
    string Name,
    string Description,
    bool RequiresFullPensum);

public sealed record CertificateCreatedDto(
    Guid CertificateId,
    Guid PaymentOrderId,
    decimal Amount,
    string Currency,
    DateTime ExpiresAt,
    string Status,
    string VerificationCode);

public sealed record CertificateDto(
    Guid Id,
    string Status,
    string VerificationCode,
    string? PdfPath,
    DateTime CreatedAt,
    DateTime? GeneratedAt,
    DateTime? SentAt
);

public sealed record CertificateVerificationDto(bool IsValid, string Message, string? StudentName, DateTime? GeneratedAt);

public sealed record CertificateSummaryDto(
    Guid Id,
    string Purpose,
    string Status,
    string PaymentStatus,
    bool PdfAvailable,
    string VerificationCode,
    Guid PaymentOrderId,
    decimal Amount,
    string Currency,
    DateTime PaymentExpiresAt,
    DateTime CreatedAt,
    DateTime? GeneratedAt,
    DateTime? SentAt);

public sealed record CertificateCancellationDto(Guid CertificateId, string Status);
