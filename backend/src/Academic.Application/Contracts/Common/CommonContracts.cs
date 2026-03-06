namespace Academic.Application.Contracts.Common;

public sealed record CampusDto(
    int Id,
    string Code,
    string Name,
    string Address,
    bool IsActive,
    string CampusType,
    string? Region);

public sealed record FilePayloadDto(byte[] Content, string ContentType, string FileName);
