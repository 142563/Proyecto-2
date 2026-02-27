using Academic.Application.Abstractions;
using Academic.Application.Common;
using Academic.Application.Contracts.Common;
using MediatR;

namespace Academic.Application.Features.Campuses;

public sealed record GetCampusesQuery : IRequest<Result<IReadOnlyList<CampusDto>>>;

public sealed class GetCampusesQueryHandler(ITransferService transferService)
    : IRequestHandler<GetCampusesQuery, Result<IReadOnlyList<CampusDto>>>
{
    public Task<Result<IReadOnlyList<CampusDto>>> Handle(GetCampusesQuery request, CancellationToken cancellationToken)
        => transferService.GetCampusesAsync(cancellationToken);
}
