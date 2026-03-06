using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence.Scaffold;

public partial class CampusShiftCapacity
{
    public long Id { get; set; }

    public int CampusId { get; set; }

    public short ShiftId { get; set; }

    public int TotalCapacity { get; set; }

    public int OccupiedCapacity { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Campus Campus { get; set; } = null!;

    public virtual Shift Shift { get; set; } = null!;
}
