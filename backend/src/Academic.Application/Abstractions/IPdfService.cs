namespace Academic.Application.Abstractions;

public interface IPdfService
{
    byte[] BuildCertificatePdf(CertificatePdfModel model);
    byte[] BuildTableReportPdf(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows);
}

public sealed record CertificatePdfModel(
    string StudentName,
    string StudentCode,
    string ProgramName,
    string Purpose,
    string VerificationCode,
    DateTime GeneratedAt,
    IReadOnlyList<string> ApprovedCourses,
    bool IncludeQr
);
