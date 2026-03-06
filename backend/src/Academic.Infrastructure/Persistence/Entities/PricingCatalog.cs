using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence;

public partial class PricingCatalog
{
    public int Id { get; set; }

    public string ServiceType { get; set; } = null!;

    public int? ProgramId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Program? Program { get; set; }
}


