using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence.Scaffold;

public partial class PaymentOrder
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public string OrderType { get; set; } = null!;

    public Guid ReferenceId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public virtual Certificate? Certificate { get; set; }

    public virtual Student Student { get; set; } = null!;
}
