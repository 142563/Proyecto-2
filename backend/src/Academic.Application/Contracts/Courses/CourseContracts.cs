namespace Academic.Application.Contracts.Courses;

public sealed record CourseDto(int Id, string Code, string Name, short Credits, bool IsOverdue, bool HasPrerequisites);

public sealed record OverdueCourseDto(int Id, string Code, string Name, short Credits);

public sealed record CreateEnrollmentDto(IReadOnlyCollection<int> CourseIds);

public sealed record EnrollmentResultDto(
    Guid EnrollmentId,
    Guid PaymentOrderId,
    decimal TotalAmount,
    string Currency,
    DateTime ExpiresAt,
    string Status);

public sealed record EnrollmentCancellationDto(Guid EnrollmentId, string Status, DateTime UpdatedAt);

public sealed record EnrollmentSummaryDto(
    Guid EnrollmentId,
    string EnrollmentType,
    string Status,
    decimal TotalAmount,
    string Currency,
    Guid PaymentOrderId,
    DateTime PaymentExpiresAt,
    DateTime CreatedAt);
