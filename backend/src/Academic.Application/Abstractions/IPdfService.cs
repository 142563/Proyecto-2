namespace Academic.Application.Abstractions;

public interface IPdfService
{
    byte[] BuildCertificatePdf(CertificatePdfModel model);
    byte[] BuildEnrollmentDirePdf(EnrollmentDirePdfModel model);
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

public sealed record EnrollmentDirePdfModel(
    string DireNumber,
    DateTime GeneratedAt,
    string StudentName,
    string Carnet,
    string StudentCode,
    string ProgramName,
    string CampusName,
    string PlanShiftName,
    string EnrollmentType,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<EnrollmentDireCourseLine> Courses
);

public sealed record EnrollmentDireCourseLine(
    string CourseCode,
    string CourseName,
    string ShiftName,
    string CourseType
);
