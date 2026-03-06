using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence.Scaffold;

public partial class Campus
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CampusType { get; set; } = null!;

    public string? Region { get; set; }

    public virtual ICollection<CampusShiftCapacity> CampusShiftCapacities { get; set; } = new List<CampusShiftCapacity>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<TransferRequest> TransferRequestFromCampuses { get; set; } = new List<TransferRequest>();

    public virtual ICollection<TransferRequest> TransferRequestToCampuses { get; set; } = new List<TransferRequest>();
}
