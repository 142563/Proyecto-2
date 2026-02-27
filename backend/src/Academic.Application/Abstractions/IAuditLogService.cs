namespace Academic.Application.Abstractions;

public interface IAuditLogService
{
    Task LogAsync(Guid? userId, string action, string entityName, string entityId, object? details, string? ipAddress, CancellationToken cancellationToken);
}
