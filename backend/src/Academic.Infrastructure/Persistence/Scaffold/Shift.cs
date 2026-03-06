using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence.Scaffold;

public partial class Shift
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CampusShiftCapacity> CampusShiftCapacities { get; set; } = new List<CampusShiftCapacity>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();
}
