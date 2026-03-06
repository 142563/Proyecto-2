using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence;

public partial class CarnetPrefixCatalog
{
    public string Prefix { get; set; } = null!;

    public int ProgramId { get; set; }

    public int CampusId { get; set; }

    public short ShiftId { get; set; }

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Program Program { get; set; } = null!;

    public virtual Campus Campus { get; set; } = null!;

    public virtual Shift Shift { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
