using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence.Scaffold;

public partial class AuditLog
{
    public long Id { get; set; }

    public Guid? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
