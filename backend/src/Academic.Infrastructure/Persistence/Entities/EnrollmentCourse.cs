using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence;

public partial class EnrollmentCourse
{
    public Guid EnrollmentId { get; set; }

    public int CourseId { get; set; }

    public bool IsOverdue { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Enrollment Enrollment { get; set; } = null!;
}


