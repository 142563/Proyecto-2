using Academic.Application.Common;
using Academic.Application.Contracts.Common;
using Academic.Application.Contracts.Transfers;

namespace Academic.Application.Abstractions;

public interface ITransferService
{
    Task<Result<IReadOnlyList<CampusDto>>> GetCampusesAsync(CancellationToken cancellationToken);
    Task<Result<TransferAvailabilityDto>> GetAvailabilityAsync(int campusId, string shift, CancellationToken cancellationToken);
    Task<Result<TransferCreateResultDto>> CreateTransferAsync(Guid studentId, CreateTransferDto request, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<TransferDto>>> GetMyTransfersAsync(Guid studentId, CancellationToken cancellationToken);
}
