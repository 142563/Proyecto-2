using Academic.Application.Abstractions;
using Academic.Application.Common;
using Academic.Application.Contracts.Transfers;
using MediatR;

namespace Academic.Application.Features.Transfers;

public sealed record GetTransferAvailabilityQuery(int CampusId, string Shift) : IRequest<Result<TransferAvailabilityDto>>;

public sealed class GetTransferAvailabilityQueryHandler(ITransferService transferService)
    : IRequestHandler<GetTransferAvailabilityQuery, Result<TransferAvailabilityDto>>
{
    public Task<Result<TransferAvailabilityDto>> Handle(GetTransferAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (request.CampusId <= 0 || string.IsNullOrWhiteSpace(request.Shift))
        {
            return Task.FromResult(Result<TransferAvailabilityDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["parameters"] = ["campusId and shift are required."]
            }));
        }

        return transferService.GetAvailabilityAsync(request.CampusId, request.Shift, cancellationToken);
    }
}

public sealed record CreateTransferCommand(CreateTransferDto Request) : IRequest<Result<TransferCreateResultDto>>;

public sealed class CreateTransferCommandHandler(ITransferService transferService, ICurrentUser currentUser)
    : IRequestHandler<CreateTransferCommand, Result<TransferCreateResultDto>>
{
    public Task<Result<TransferCreateResultDto>> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<TransferCreateResultDto>.Failure("forbidden", "Only students can create transfer requests."));
        }

        if (request.Request.CampusId <= 0 || string.IsNullOrWhiteSpace(request.Request.Shift))
        {
            return Task.FromResult(Result<TransferCreateResultDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["request"] = ["CampusId and Shift are required."]
            }));
        }

        return transferService.CreateTransferAsync(currentUser.StudentId.Value, request.Request, cancellationToken);
    }
}

public sealed record GetMyTransfersQuery : IRequest<Result<IReadOnlyList<TransferDto>>>;

public sealed class GetMyTransfersQueryHandler(ITransferService transferService, ICurrentUser currentUser)
    : IRequestHandler<GetMyTransfersQuery, Result<IReadOnlyList<TransferDto>>>
{
    public Task<Result<IReadOnlyList<TransferDto>>> Handle(GetMyTransfersQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<IReadOnlyList<TransferDto>>.Failure("forbidden", "Only students can query transfer requests."));
        }

        return transferService.GetMyTransfersAsync(currentUser.StudentId.Value, cancellationToken);
    }
}

public sealed record CancelTransferCommand(Guid TransferId) : IRequest<Result<TransferCancellationDto>>;

public sealed class CancelTransferCommandHandler(ITransferService transferService, ICurrentUser currentUser)
    : IRequestHandler<CancelTransferCommand, Result<TransferCancellationDto>>
{
    public Task<Result<TransferCancellationDto>> Handle(CancelTransferCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<TransferCancellationDto>.Failure("forbidden", "Only students can cancel transfer requests."));
        }

        return transferService.CancelTransferAsync(currentUser.StudentId.Value, request.TransferId, cancellationToken);
    }
}

public sealed record ReviewTransferCommand(Guid TransferId, ReviewTransferDto Request) : IRequest<Result<TransferReviewResultDto>>;

public sealed class ReviewTransferCommandHandler(ITransferService transferService, ICurrentUser currentUser)
    : IRequestHandler<ReviewTransferCommand, Result<TransferReviewResultDto>>
{
    public Task<Result<TransferReviewResultDto>> Handle(ReviewTransferCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Task.FromResult(Result<TransferReviewResultDto>.Failure("unauthorized", "User is not authenticated."));
        }

        if (string.IsNullOrWhiteSpace(request.Request.Decision))
        {
            return Task.FromResult(Result<TransferReviewResultDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["decision"] = ["Decision is required."]
            }));
        }

        return transferService.ReviewTransferAsync(currentUser.UserId.Value, request.TransferId, request.Request, cancellationToken);
    }
}
