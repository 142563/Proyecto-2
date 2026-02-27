using Academic.Application.Abstractions;
using Academic.Application.Common;
using Academic.Application.Contracts.Auth;
using MediatR;

namespace Academic.Application.Features.Auth;

public sealed record LoginCommand(LoginRequestDto Request) : IRequest<Result<AuthResponseDto>>;

public sealed class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    public Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Request.Email) || string.IsNullOrWhiteSpace(request.Request.Password))
        {
            return Task.FromResult(Result<AuthResponseDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["credentials"] = ["Email and password are required."]
            }));
        }

        return authService.LoginAsync(request.Request.Email, request.Request.Password, cancellationToken);
    }
}

public sealed record GetMeQuery : IRequest<Result<MeDto>>;

public sealed class GetMeQueryHandler(IAuthService authService, ICurrentUser currentUser) : IRequestHandler<GetMeQuery, Result<MeDto>>
{
    public Task<Result<MeDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Task.FromResult(Result<MeDto>.Failure("unauthorized", "User is not authenticated."));
        }

        return authService.GetMeAsync(currentUser.UserId.Value, cancellationToken);
    }
}
