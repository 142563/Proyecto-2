using System;
using System.Collections.Generic;

namespace Academic.Infrastructure.Persistence;

public partial class Student
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string InstitutionalEmail { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Carnet { get; set; } = null!;

    public string CarnetPrefix { get; set; } = null!;

    public short EntryYear { get; set; }

    public string CarnetSequence { get; set; } = null!;

    public int ProgramId { get; set; }

    public int? CurrentCampusId { get; set; }

    public short? CurrentShiftId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual CarnetPrefixCatalog CarnetPrefixNavigation { get; set; } = null!;

    public virtual Campus? CurrentCampus { get; set; }

    public virtual Shift? CurrentShift { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();

    public virtual Program Program { get; set; } = null!;

    public virtual ICollection<StudentCourseHistory> StudentCourseHistories { get; set; } = new List<StudentCourseHistory>();

    public virtual ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();

    public virtual User User { get; set; } = null!;
}


