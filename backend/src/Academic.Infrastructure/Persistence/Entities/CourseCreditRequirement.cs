using System;

namespace Academic.Infrastructure.Persistence;

public partial class CourseCreditRequirement
{
    public long Id { get; set; }

    public int CourseId { get; set; }

    public short MinApprovedCredits { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;
}
