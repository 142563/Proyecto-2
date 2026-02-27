using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence.Scaffold;

public partial class StudentCourseHistory
{
    public long Id { get; set; }

    public Guid StudentId { get; set; }

    public int CourseId { get; set; }

    public short Year { get; set; }

    public string Term { get; set; } = null!;

    public decimal? Grade { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
