using Academic.Application.Abstractions;
using Academic.Application.Common;
using Academic.Application.Contracts.Courses;
using MediatR;

namespace Academic.Application.Features.Courses;

public sealed record GetPensumQuery : IRequest<Result<IReadOnlyList<CourseDto>>>;

public sealed class GetPensumQueryHandler(ICourseService courseService, ICurrentUser currentUser)
    : IRequestHandler<GetPensumQuery, Result<IReadOnlyList<CourseDto>>>
{
    public Task<Result<IReadOnlyList<CourseDto>>> Handle(GetPensumQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<IReadOnlyList<CourseDto>>.Failure("forbidden", "Only students can query courses."));
        }

        return courseService.GetPensumAsync(currentUser.StudentId.Value, cancellationToken);
    }
}

public sealed record GetOverdueCoursesQuery : IRequest<Result<IReadOnlyList<OverdueCourseDto>>>;

public sealed class GetOverdueCoursesQueryHandler(ICourseService courseService, ICurrentUser currentUser)
    : IRequestHandler<GetOverdueCoursesQuery, Result<IReadOnlyList<OverdueCourseDto>>>
{
    public Task<Result<IReadOnlyList<OverdueCourseDto>>> Handle(GetOverdueCoursesQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<IReadOnlyList<OverdueCourseDto>>.Failure("forbidden", "Only students can query overdue courses."));
        }

        return courseService.GetOverdueAsync(currentUser.StudentId.Value, cancellationToken);
    }
}

public sealed record GetMyEnrollmentsQuery : IRequest<Result<IReadOnlyList<EnrollmentSummaryDto>>>;

public sealed class GetMyEnrollmentsQueryHandler(ICourseService courseService, ICurrentUser currentUser)
    : IRequestHandler<GetMyEnrollmentsQuery, Result<IReadOnlyList<EnrollmentSummaryDto>>>
{
    public Task<Result<IReadOnlyList<EnrollmentSummaryDto>>> Handle(GetMyEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<IReadOnlyList<EnrollmentSummaryDto>>.Failure("forbidden", "Only students can query enrollments."));
        }

        return courseService.GetMyEnrollmentsAsync(currentUser.StudentId.Value, cancellationToken);
    }
}

public sealed record CreateEnrollmentCommand(CreateEnrollmentDto Request) : IRequest<Result<EnrollmentResultDto>>;

public sealed class CreateEnrollmentCommandHandler(ICourseService courseService, ICurrentUser currentUser)
    : IRequestHandler<CreateEnrollmentCommand, Result<EnrollmentResultDto>>
{
    public Task<Result<EnrollmentResultDto>> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<EnrollmentResultDto>.Failure("forbidden", "Only students can create enrollments."));
        }

        if (request.Request.CourseSelections is null || request.Request.CourseSelections.Count == 0)
        {
            return Task.FromResult(Result<EnrollmentResultDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["courseSelections"] = ["Debe seleccionar al menos un curso."]
            }));
        }

        return courseService.CreateEnrollmentAsync(currentUser.StudentId.Value, request.Request, cancellationToken);
    }
}

public sealed record CancelEnrollmentCommand(Guid EnrollmentId) : IRequest<Result<EnrollmentCancellationDto>>;

public sealed class CancelEnrollmentCommandHandler(ICourseService courseService, ICurrentUser currentUser)
    : IRequestHandler<CancelEnrollmentCommand, Result<EnrollmentCancellationDto>>
{
    public Task<Result<EnrollmentCancellationDto>> Handle(CancelEnrollmentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<EnrollmentCancellationDto>.Failure("forbidden", "Only students can cancel enrollments."));
        }

        return courseService.CancelEnrollmentAsync(currentUser.StudentId.Value, request.EnrollmentId, cancellationToken);
    }
}
