namespace Academic.Application.Contracts.Payments;

public sealed record PaymentOrderDto(
    Guid Id,
    string OrderType,
    Guid ReferenceId,
    decimal Amount,
    string Currency,
    string Status,
    string Description,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? PaidAt,
    DateTime? CancelledAt
);
