using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence.Scaffold;

public partial class TransferRequest
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public int? FromCampusId { get; set; }

    public int ToCampusId { get; set; }

    public short ToShiftId { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewNotes { get; set; }

    public virtual Campus? FromCampus { get; set; }

    public virtual User? ReviewedByUser { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Campus ToCampus { get; set; } = null!;

    public virtual Shift ToShift { get; set; } = null!;
}
