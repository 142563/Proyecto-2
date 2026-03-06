namespace Academic.Application.Contracts.Auth;

public sealed record LoginRequestDto(string Email, string Password);

public sealed record JwtTokenContext(Guid UserId, Guid? StudentId, string Email, string Role);

public sealed record AuthResponseDto(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Role,
    Guid UserId,
    Guid? StudentId,
    string Email
);

public sealed record MeDto(
    Guid UserId,
    Guid? StudentId,
    string Email,
    string Role,
    bool IsActive,
    string? FullName,
    string? Carnet,
    string? ProgramName,
    string? CampusName,
    string? ShiftName);

