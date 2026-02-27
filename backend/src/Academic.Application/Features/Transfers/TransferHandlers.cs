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
