using Academic.Application.Contracts.Auth;

namespace Academic.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateToken(JwtTokenContext context);
}
