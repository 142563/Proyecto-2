using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence;

public partial class Certificate
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid PaymentOrderId { get; set; }

    public string Purpose { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string VerificationCode { get; set; } = null!;

    public string? PdfPath { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual PaymentOrder PaymentOrder { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}


