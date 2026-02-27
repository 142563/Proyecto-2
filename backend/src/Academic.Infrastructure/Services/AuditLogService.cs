using System.Text.Json;
using Academic.Application.Abstractions;
using Academic.Infrastructure.Persistence.Scaffold;

namespace Academic.Infrastructure.Services;

public sealed class AuditLogService(AcademicDbContext dbContext) : IAuditLogService
{
    public async Task LogAsync(
        Guid? userId,
        string action,
        string entityName,
        string entityId,
        object? details,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var log = new AuditLog
        {
            Id = 0,
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details is null ? null : JsonSerializer.Serialize(details),
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.AuditLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
