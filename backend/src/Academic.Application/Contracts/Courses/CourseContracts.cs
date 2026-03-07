namespace Academic.Application.Contracts.Courses;

public sealed record CourseDto(
    int Id,
    string Code,
    string Name,
    short Credits,
    short Cycle,
    short HoursPerWeek,
    short HoursTotal,
    bool IsLab,
    bool IsApproved,
    bool IsOverdue,
    bool HasPrerequisites,
    string PrerequisiteSummary);

public sealed record OverdueCourseDto(int Id, string Code, string Name, short Credits);

public sealed record EnrollmentCourseSelectionDto(int CourseId, string Shift);

public sealed record CreateEnrollmentDto(IReadOnlyCollection<EnrollmentCourseSelectionDto> CourseSelections);

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

