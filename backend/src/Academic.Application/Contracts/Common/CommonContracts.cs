namespace Academic.Application.Contracts.Common;

public sealed record CampusDto(int Id, string Code, string Name, string Address, bool IsActive);

public sealed record FilePayloadDto(byte[] Content, string ContentType, string FileName);
