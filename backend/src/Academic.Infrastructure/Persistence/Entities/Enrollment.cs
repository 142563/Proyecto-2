using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence;

public partial class Enrollment
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public string EnrollmentType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<EnrollmentCourse> EnrollmentCourses { get; set; } = new List<EnrollmentCourse>();

    public virtual Student Student { get; set; } = null!;
}


