using Academic.Application.Common;
using Academic.Application.Contracts.Auth;

namespace Academic.Application.Abstractions;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<MeDto>> GetMeAsync(Guid userId, CancellationToken cancellationToken);
}
