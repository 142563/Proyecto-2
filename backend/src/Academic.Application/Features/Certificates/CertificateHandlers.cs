using Academic.Application.Abstractions;
using Academic.Application.Common;
using Academic.Application.Contracts.Certificates;
using Academic.Application.Contracts.Common;
using MediatR;

namespace Academic.Application.Features.Certificates;

public sealed record CreateCertificateCommand(CreateCertificateDto Request) : IRequest<Result<CertificateCreatedDto>>;

public sealed class CreateCertificateCommandHandler(ICertificateService certificateService, ICurrentUser currentUser)
    : IRequestHandler<CreateCertificateCommand, Result<CertificateCreatedDto>>
{
    public Task<Result<CertificateCreatedDto>> Handle(CreateCertificateCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<CertificateCreatedDto>.Failure("forbidden", "Only students can request certificates."));
        }

        if (string.IsNullOrWhiteSpace(request.Request.Purpose))
        {
            return Task.FromResult(Result<CertificateCreatedDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["purpose"] = ["Purpose is required."]
            }));
        }

        return certificateService.CreateAsync(currentUser.StudentId.Value, request.Request, cancellationToken);
    }
}

public sealed record GenerateCertificateCommand(Guid CertificateId, GenerateCertificateDto Request) : IRequest<Result<CertificateDto>>;

public sealed class GenerateCertificateCommandHandler(ICertificateService certificateService, ICurrentUser currentUser)
    : IRequestHandler<GenerateCertificateCommand, Result<CertificateDto>>
{
    public Task<Result<CertificateDto>> Handle(GenerateCertificateCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<CertificateDto>.Failure("forbidden", "Only students can generate their certificates."));
        }

        return certificateService.GenerateAsync(currentUser.StudentId.Value, request.CertificateId, request.Request, cancellationToken);
    }
}

public sealed record DownloadCertificateQuery(Guid CertificateId) : IRequest<Result<FilePayloadDto>>;

public sealed class DownloadCertificateQueryHandler(ICertificateService certificateService, ICurrentUser currentUser)
    : IRequestHandler<DownloadCertificateQuery, Result<FilePayloadDto>>
{
    public Task<Result<FilePayloadDto>> Handle(DownloadCertificateQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<FilePayloadDto>.Failure("forbidden", "Only students can download certificates."));
        }

        return certificateService.DownloadAsync(currentUser.StudentId.Value, request.CertificateId, cancellationToken);
    }
}

public sealed record VerifyCertificateQuery(string VerificationCode) : IRequest<Result<CertificateVerificationDto>>;

public sealed class VerifyCertificateQueryHandler(ICertificateService certificateService)
    : IRequestHandler<VerifyCertificateQuery, Result<CertificateVerificationDto>>
{
    public Task<Result<CertificateVerificationDto>> Handle(VerifyCertificateQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.VerificationCode))
        {
            return Task.FromResult(Result<CertificateVerificationDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["verificationCode"] = ["Verification code is required."]
            }));
        }

        return certificateService.VerifyAsync(request.VerificationCode, cancellationToken);
    }
}

public sealed record GetMyCertificatesQuery : IRequest<Result<IReadOnlyList<CertificateSummaryDto>>>;

public sealed class GetMyCertificatesQueryHandler(ICertificateService certificateService, ICurrentUser currentUser)
    : IRequestHandler<GetMyCertificatesQuery, Result<IReadOnlyList<CertificateSummaryDto>>>
{
    public Task<Result<IReadOnlyList<CertificateSummaryDto>>> Handle(GetMyCertificatesQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<IReadOnlyList<CertificateSummaryDto>>.Failure("forbidden", "Only students can query certificates."));
        }

        return certificateService.GetMyCertificatesAsync(currentUser.StudentId.Value, cancellationToken);
    }
}

public sealed record CancelCertificateCommand(Guid CertificateId) : IRequest<Result<CertificateCancellationDto>>;

public sealed class CancelCertificateCommandHandler(ICertificateService certificateService, ICurrentUser currentUser)
    : IRequestHandler<CancelCertificateCommand, Result<CertificateCancellationDto>>
{
    public Task<Result<CertificateCancellationDto>> Handle(CancelCertificateCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<CertificateCancellationDto>.Failure("forbidden", "Only students can cancel certificates."));
        }

        return certificateService.CancelAsync(currentUser.StudentId.Value, request.CertificateId, cancellationToken);
    }
}
