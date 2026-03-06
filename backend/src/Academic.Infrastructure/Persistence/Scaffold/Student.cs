using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence.Scaffold;

public partial class Student
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string InstitutionalEmail { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int ProgramId { get; set; }

    public int? CurrentCampusId { get; set; }

    public short? CurrentShiftId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Certificate? Certificate { get; set; }

    public virtual Campus? CurrentCampus { get; set; }

    public virtual Shift? CurrentShift { get; set; }

    public virtual Enrollment? Enrollment { get; set; }

    public virtual ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();

    public virtual Program Program { get; set; } = null!;

    public virtual ICollection<StudentCourseHistory> StudentCourseHistories { get; set; } = new List<StudentCourseHistory>();

    public virtual TransferRequest? TransferRequest { get; set; }

    public virtual User User { get; set; } = null!;
}
