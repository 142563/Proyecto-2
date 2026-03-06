using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence.Scaffold;

public partial class Program
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<PricingCatalog> PricingCatalogs { get; set; } = new List<PricingCatalog>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
