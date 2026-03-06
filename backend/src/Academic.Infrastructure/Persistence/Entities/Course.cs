using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence;

public partial class Course
{
    public int Id { get; set; }

    public int ProgramId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public short Credits { get; set; }

    public short Cycle { get; set; }

    public short HoursPerWeek { get; set; }

    public short HoursTotal { get; set; }

    public bool IsLab { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<EnrollmentCourse> EnrollmentCourses { get; set; } = new List<EnrollmentCourse>();

    public virtual Program Program { get; set; } = null!;

    public virtual ICollection<StudentCourseHistory> StudentCourseHistories { get; set; } = new List<StudentCourseHistory>();

    public virtual ICollection<CourseCreditRequirement> CourseCreditRequirements { get; set; } = new List<CourseCreditRequirement>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Course> PrerequisiteCourses { get; set; } = new List<Course>();
}


