using Academic.Application.Common;
using Academic.Application.Contracts.Payments;

namespace Academic.Application.Abstractions;

public interface IPaymentService
{
    Task<Result<IReadOnlyList<PaymentOrderDto>>> GetMyPaymentsAsync(Guid studentId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<PaymentOrderDto>>> GetPendingPaymentsAsync(CancellationToken cancellationToken);
    Task<Result<PaymentOrderDto>> MarkPaidAsync(Guid paymentId, Guid actedByUserId, CancellationToken cancellationToken);
    Task<Result<MockCheckoutResultDto>> MockCheckoutAsync(
        Guid paymentId,
        Guid studentId,
        Guid actedByUserId,
        MockCheckoutRequestDto request,
        CancellationToken cancellationToken);
}
