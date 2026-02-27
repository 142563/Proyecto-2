using Academic.Application.Abstractions;
using Academic.Application.Common;
using Academic.Application.Contracts.Payments;
using MediatR;

namespace Academic.Application.Features.Payments;

public sealed record GetMyPaymentsQuery : IRequest<Result<IReadOnlyList<PaymentOrderDto>>>;

public sealed class GetMyPaymentsQueryHandler(IPaymentService paymentService, ICurrentUser currentUser)
    : IRequestHandler<GetMyPaymentsQuery, Result<IReadOnlyList<PaymentOrderDto>>>
{
    public Task<Result<IReadOnlyList<PaymentOrderDto>>> Handle(GetMyPaymentsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.StudentId.HasValue)
        {
            return Task.FromResult(Result<IReadOnlyList<PaymentOrderDto>>.Failure("forbidden", "Only students can query payments."));
        }

        return paymentService.GetMyPaymentsAsync(currentUser.StudentId.Value, cancellationToken);
    }
}

public sealed record MarkPaymentPaidCommand(Guid PaymentId) : IRequest<Result<PaymentOrderDto>>;

public sealed class MarkPaymentPaidCommandHandler(IPaymentService paymentService, ICurrentUser currentUser)
    : IRequestHandler<MarkPaymentPaidCommand, Result<PaymentOrderDto>>
{
    public Task<Result<PaymentOrderDto>> Handle(MarkPaymentPaidCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Task.FromResult(Result<PaymentOrderDto>.Failure("unauthorized", "User is not authenticated."));
        }

        return paymentService.MarkPaidAsync(request.PaymentId, currentUser.UserId.Value, cancellationToken);
    }
}
