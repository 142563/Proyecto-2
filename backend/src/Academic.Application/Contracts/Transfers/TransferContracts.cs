namespace Academic.Application.Contracts.Transfers;

public sealed record CreateTransferDto(int CampusId, string Shift, string? Reason);

public sealed record TransferAvailabilityDto(
    int CampusId,
    string CampusName,
    short ShiftId,
    string ShiftName,
    int TotalCapacity,
    int OccupiedCapacity,
    int AvailableCapacity
);

public sealed record TransferCreateResultDto(Guid TransferId, Guid PaymentOrderId, decimal Amount, string TransferStatus);

public sealed record TransferDto(
    Guid TransferId,
    string StudentCode,
    string StudentName,
    string FromCampus,
    string ToCampus,
    string Shift,
    string Status,
    DateTime CreatedAt
);
