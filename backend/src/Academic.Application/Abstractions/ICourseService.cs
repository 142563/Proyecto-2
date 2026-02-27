using Academic.Application.Common;
using Academic.Application.Contracts.Courses;

namespace Academic.Application.Abstractions;

public interface ICourseService
{
    Task<Result<IReadOnlyList<CourseDto>>> GetPensumAsync(Guid studentId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<OverdueCourseDto>>> GetOverdueAsync(Guid studentId, CancellationToken cancellationToken);
    Task<Result<EnrollmentResultDto>> CreateEnrollmentAsync(Guid studentId, CreateEnrollmentDto request, CancellationToken cancellationToken);
}
