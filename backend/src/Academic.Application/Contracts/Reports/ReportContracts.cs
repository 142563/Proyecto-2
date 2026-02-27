namespace Academic.Application.Contracts.Reports;

public sealed record TransferReportRowDto(
    Guid TransferId,
    string StudentCode,
    string StudentName,
    string FromCampus,
    string ToCampus,
    string Shift,
    string Status,
    DateTime CreatedAt
);

public sealed record EnrollmentReportRowDto(
    Guid EnrollmentId,
    string StudentCode,
    string StudentName,
    string EnrollmentType,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt
);

public sealed record CertificateReportRowDto(
    Guid CertificateId,
    string StudentCode,
    string StudentName,
    string Status,
    string VerificationCode,
    DateTime CreatedAt,
    DateTime? GeneratedAt
);
