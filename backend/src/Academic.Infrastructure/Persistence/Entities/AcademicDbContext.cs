using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Academic.Infrastructure.Persistence;

public partial class AcademicDbContext : DbContext
{
    public AcademicDbContext(DbContextOptions<AcademicDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Campus> Campuses { get; set; }

    public virtual DbSet<CarnetPrefixCatalog> CarnetPrefixCatalogs { get; set; }

    public virtual DbSet<CampusShiftCapacity> CampusShiftCapacities { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseCreditRequirement> CourseCreditRequirements { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<EnrollmentCourse> EnrollmentCourses { get; set; }

    public virtual DbSet<PaymentOrder> PaymentOrders { get; set; }

    public virtual DbSet<PricingCatalog> PricingCatalogs { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentCourseHistory> StudentCourseHistories { get; set; }

    public virtual DbSet<TransferRequest> TransferRequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audit_logs_pkey");

            entity.ToTable("audit_logs");

            entity.HasIndex(e => new { e.EntityName, e.EntityId, e.CreatedAt }, "idx_audit_logs_entity_created").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_audit_logs_user_created").IsDescending(false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Details)
                .HasColumnType("jsonb")
                .HasColumnName("details");
            entity.Property(e => e.EntityId)
                .HasMaxLength(100)
                .HasColumnName("entity_id");
            entity.Property(e => e.EntityName)
                .HasMaxLength(100)
                .HasColumnName("entity_name");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(64)
                .HasColumnName("ip_address");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("audit_logs_user_id_fkey");
        });

        modelBuilder.Entity<Campus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("campuses_pkey");

            entity.ToTable("campuses");

            entity.HasIndex(e => e.Code, "campuses_code_key").IsUnique();

            entity.HasIndex(e => e.Name, "campuses_name_key").IsUnique();

            entity.HasIndex(e => new { e.CampusType, e.Region }, "idx_campuses_type_region");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(280)
                .HasColumnName("address");
            entity.Property(e => e.CampusType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Campus'::character varying")
                .HasColumnName("campus_type");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(180)
                .HasColumnName("name");
            entity.Property(e => e.Region)
                .HasMaxLength(60)
                .HasColumnName("region");
        });

        modelBuilder.Entity<CarnetPrefixCatalog>(entity =>
        {
            entity.HasKey(e => e.Prefix).HasName("carnet_prefix_catalog_pkey");

            entity.ToTable("carnet_prefix_catalog");

            entity.HasIndex(e => e.CampusId, "idx_carnet_prefix_campus_id");
            entity.HasIndex(e => e.ProgramId, "idx_carnet_prefix_program_id");
            entity.HasIndex(e => e.ShiftId, "idx_carnet_prefix_shift_id");

            entity.Property(e => e.Prefix)
                .HasMaxLength(4)
                .HasColumnName("prefix");
            entity.Property(e => e.CampusId).HasColumnName("campus_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(180)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");

            entity.HasOne(d => d.Campus).WithMany(p => p.CarnetPrefixCatalogs)
                .HasForeignKey(d => d.CampusId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("carnet_prefix_catalog_campus_id_fkey");

            entity.HasOne(d => d.Program).WithMany(p => p.CarnetPrefixCatalogs)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("carnet_prefix_catalog_program_id_fkey");

            entity.HasOne(d => d.Shift).WithMany(p => p.CarnetPrefixCatalogs)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("carnet_prefix_catalog_shift_id_fkey");
        });

        modelBuilder.Entity<CampusShiftCapacity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("campus_shift_capacity_pkey");

            entity.ToTable("campus_shift_capacity");

            entity.HasIndex(e => new { e.CampusId, e.ShiftId }, "uq_campus_shift").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CampusId).HasColumnName("campus_id");
            entity.Property(e => e.OccupiedCapacity)
                .HasDefaultValue(0)
                .HasColumnName("occupied_capacity");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.TotalCapacity).HasColumnName("total_capacity");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Campus).WithMany(p => p.CampusShiftCapacities)
                .HasForeignKey(d => d.CampusId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("campus_shift_capacity_campus_id_fkey");

            entity.HasOne(d => d.Shift).WithMany(p => p.CampusShiftCapacities)
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("campus_shift_capacity_shift_id_fkey");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("certificates_pkey");

            entity.ToTable("certificates");

            entity.HasIndex(e => e.PaymentOrderId, "certificates_payment_order_id_key").IsUnique();

            entity.HasIndex(e => e.VerificationCode, "certificates_verification_code_key").IsUnique();

            entity.HasIndex(e => e.VerificationCode, "idx_certificates_code");

            entity.HasIndex(e => new { e.StudentId, e.Status }, "idx_certificates_student_status");

            entity.HasIndex(e => e.StudentId, "uq_certificate_active_per_student")
                .IsUnique()
                .HasFilter("((status)::text = ANY ((ARRAY['Requested'::character varying, 'PdfGenerated'::character varying])::text[]))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GeneratedAt).HasColumnName("generated_at");
            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb")
                .HasColumnName("metadata");
            entity.Property(e => e.PaymentOrderId).HasColumnName("payment_order_id");
            entity.Property(e => e.PdfPath)
                .HasMaxLength(500)
                .HasColumnName("pdf_path");
            entity.Property(e => e.Purpose)
                .HasMaxLength(255)
                .HasColumnName("purpose");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.VerificationCode)
                .HasMaxLength(60)
                .HasColumnName("verification_code");

            entity.HasOne(d => d.PaymentOrder).WithOne(p => p.Certificate)
                .HasForeignKey<Certificate>(d => d.PaymentOrderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("certificates_payment_order_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("certificates_student_id_fkey");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("courses_pkey");

            entity.ToTable("courses");

            entity.HasIndex(e => e.Code, "courses_code_key").IsUnique();

            entity.HasIndex(e => e.ProgramId, "idx_courses_program_id");

            entity.HasIndex(e => new { e.ProgramId, e.Cycle }, "idx_courses_program_cycle");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Credits).HasColumnName("credits");
            entity.Property(e => e.Cycle).HasColumnName("cycle");
            entity.Property(e => e.HoursPerWeek).HasColumnName("hours_per_week");
            entity.Property(e => e.HoursTotal).HasColumnName("hours_total");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsLab)
                .HasDefaultValue(false)
                .HasColumnName("is_lab");
            entity.Property(e => e.Name)
                .HasMaxLength(180)
                .HasColumnName("name");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");

            entity.HasOne(d => d.Program).WithMany(p => p.Courses)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("courses_program_id_fkey");

            entity.HasMany(d => d.Courses).WithMany(p => p.PrerequisiteCourses)
                .UsingEntity<Dictionary<string, object>>(
                    "CoursePrerequisite",
                    r => r.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .HasConstraintName("course_prerequisites_course_id_fkey"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("PrerequisiteCourseId")
                        .HasConstraintName("course_prerequisites_prerequisite_course_id_fkey"),
                    j =>
                    {
                        j.HasKey("CourseId", "PrerequisiteCourseId").HasName("course_prerequisites_pkey");
                        j.ToTable("course_prerequisites");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                        j.IndexerProperty<int>("PrerequisiteCourseId").HasColumnName("prerequisite_course_id");
                    });

            entity.HasMany(d => d.PrerequisiteCourses).WithMany(p => p.Courses)
                .UsingEntity<Dictionary<string, object>>(
                    "CoursePrerequisite",
                    r => r.HasOne<Course>().WithMany()
                        .HasForeignKey("PrerequisiteCourseId")
                        .HasConstraintName("course_prerequisites_prerequisite_course_id_fkey"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .HasConstraintName("course_prerequisites_course_id_fkey"),
                    j =>
                    {
                        j.HasKey("CourseId", "PrerequisiteCourseId").HasName("course_prerequisites_pkey");
                        j.ToTable("course_prerequisites");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                        j.IndexerProperty<int>("PrerequisiteCourseId").HasColumnName("prerequisite_course_id");
                    });
        });

        modelBuilder.Entity<CourseCreditRequirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("course_credit_requirements_pkey");

            entity.ToTable("course_credit_requirements");

            entity.HasIndex(e => e.CourseId, "idx_course_credit_requirements_course_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.MinApprovedCredits).HasColumnName("min_approved_credits");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseCreditRequirements)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("course_credit_requirements_course_id_fkey");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("enrollments_pkey");

            entity.ToTable("enrollments");

            entity.HasIndex(e => new { e.StudentId, e.Status }, "idx_enrollments_student_status");

            entity.HasIndex(e => e.StudentId, "uq_enrollment_active_per_student")
                .IsUnique()
                .HasFilter("((status)::text = 'PendingPayment'::text)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EnrollmentType)
                .HasMaxLength(20)
                .HasColumnName("enrollment_type");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("enrollments_student_id_fkey");
        });

        modelBuilder.Entity<EnrollmentCourse>(entity =>
        {
            entity.HasKey(e => new { e.EnrollmentId, e.CourseId }).HasName("enrollment_courses_pkey");

            entity.ToTable("enrollment_courses");

            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.IsOverdue)
                .HasDefaultValue(false)
                .HasColumnName("is_overdue");

            entity.HasOne(d => d.Course).WithMany(p => p.EnrollmentCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("enrollment_courses_course_id_fkey");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.EnrollmentCourses)
                .HasForeignKey(d => d.EnrollmentId)
                .HasConstraintName("enrollment_courses_enrollment_id_fkey");
        });

        modelBuilder.Entity<PaymentOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_orders_pkey");

            entity.ToTable("payment_orders");

            entity.HasIndex(e => new { e.ReferenceId, e.OrderType }, "idx_payment_orders_reference");

            entity.HasIndex(e => new { e.Status, e.ExpiresAt }, "idx_payment_orders_status_expires");

            entity.HasIndex(e => new { e.StudentId, e.Status }, "idx_payment_orders_student_status");

            entity.HasIndex(e => new { e.OrderType, e.ReferenceId }, "uq_payment_reference").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'GTQ'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.ExpiresAt)
                .HasDefaultValueSql("(now() + '72:00:00'::interval)")
                .HasColumnName("expires_at");
            entity.Property(e => e.OrderType)
                .HasMaxLength(20)
                .HasColumnName("order_type");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Student).WithMany(p => p.PaymentOrders)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("payment_orders_student_id_fkey");
        });

        modelBuilder.Entity<PricingCatalog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pricing_catalog_pkey");

            entity.ToTable("pricing_catalog");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValueSql("'GTQ'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.ServiceType)
                .HasMaxLength(30)
                .HasColumnName("service_type");

            entity.HasOne(d => d.Program).WithMany(p => p.PricingCatalogs)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("pricing_catalog_program_id_fkey");
        });

        modelBuilder.Entity<Program>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("programs_pkey");

            entity.ToTable("programs");

            entity.HasIndex(e => e.Code, "programs_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(180)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(32)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shifts_pkey");

            entity.ToTable("shifts");

            entity.HasIndex(e => e.Name, "shifts_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("students_pkey");

            entity.ToTable("students");

            entity.HasIndex(e => e.ProgramId, "idx_students_program_id");

            entity.HasIndex(e => e.UserId, "idx_students_user_id");

            entity.HasIndex(e => e.InstitutionalEmail, "students_institutional_email_key").IsUnique();

            entity.HasIndex(e => e.Carnet, "students_carnet_key").IsUnique();

            entity.HasIndex(e => e.StudentCode, "students_student_code_key").IsUnique();

            entity.HasIndex(e => e.UserId, "students_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Carnet)
                .HasMaxLength(20)
                .HasColumnName("carnet");
            entity.Property(e => e.CarnetPrefix)
                .HasMaxLength(4)
                .HasColumnName("carnet_prefix");
            entity.Property(e => e.CarnetSequence)
                .HasMaxLength(5)
                .HasColumnName("carnet_sequence");
            entity.Property(e => e.CurrentCampusId).HasColumnName("current_campus_id");
            entity.Property(e => e.CurrentShiftId).HasColumnName("current_shift_id");
            entity.Property(e => e.EntryYear).HasColumnName("entry_year");
            entity.Property(e => e.FirstName)
                .HasMaxLength(120)
                .HasColumnName("first_name");
            entity.Property(e => e.InstitutionalEmail)
                .HasMaxLength(255)
                .HasColumnName("institutional_email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(120)
                .HasColumnName("last_name");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.StudentCode)
                .HasMaxLength(30)
                .HasColumnName("student_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.CurrentCampus).WithMany(p => p.Students)
                .HasForeignKey(d => d.CurrentCampusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("students_current_campus_id_fkey");

            entity.HasOne(d => d.CurrentShift).WithMany(p => p.Students)
                .HasForeignKey(d => d.CurrentShiftId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("students_current_shift_id_fkey");

            entity.HasOne(d => d.CarnetPrefixNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.CarnetPrefix)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("students_carnet_prefix_fkey");

            entity.HasOne(d => d.Program).WithMany(p => p.Students)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("students_program_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .HasConstraintName("students_user_id_fkey");
        });

        modelBuilder.Entity<StudentCourseHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("student_course_history_pkey");

            entity.ToTable("student_course_history");

            entity.HasIndex(e => new { e.CourseId, e.StudentId }, "idx_history_course_student");

            entity.HasIndex(e => new { e.StudentId, e.Status }, "idx_history_student_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Grade)
                .HasPrecision(5, 2)
                .HasColumnName("grade");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.Term)
                .HasMaxLength(20)
                .HasColumnName("term");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Course).WithMany(p => p.StudentCourseHistories)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("student_course_history_course_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentCourseHistories)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("student_course_history_student_id_fkey");
        });

        modelBuilder.Entity<TransferRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transfer_requests_pkey");

            entity.ToTable("transfer_requests");

            entity.HasIndex(e => new { e.ToCampusId, e.ToShiftId, e.Status }, "idx_transfers_destination");

            entity.HasIndex(e => new { e.StudentId, e.Status }, "idx_transfers_student_status");

            entity.HasIndex(e => e.StudentId, "uq_transfer_active_per_student")
                .IsUnique()
                .HasFilter("((status)::text = ANY ((ARRAY['PendingPayment'::character varying, 'PendingReview'::character varying])::text[]))");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FromCampusId).HasColumnName("from_campus_id");
            entity.Property(e => e.Modality)
                .HasMaxLength(20)
                .HasColumnName("modality");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .HasColumnName("reason");
            entity.Property(e => e.ReviewNotes)
                .HasMaxLength(500)
                .HasColumnName("review_notes");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedByUserId).HasColumnName("reviewed_by_user_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.ToCampusId).HasColumnName("to_campus_id");
            entity.Property(e => e.ToShiftId).HasColumnName("to_shift_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.FromCampus).WithMany(p => p.TransferRequestFromCampuses)
                .HasForeignKey(d => d.FromCampusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transfer_requests_from_campus_id_fkey");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.TransferRequests)
                .HasForeignKey(d => d.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transfer_requests_reviewed_by_user_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.TransferRequests)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("transfer_requests_student_id_fkey");

            entity.HasOne(d => d.ToCampus).WithMany(p => p.TransferRequestToCampuses)
                .HasForeignKey(d => d.ToCampusId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transfer_requests_to_campus_id_fkey");

            entity.HasOne(d => d.ToShift).WithMany(p => p.TransferRequests)
                .HasForeignKey(d => d.ToShiftId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("transfer_requests_to_shift_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("user_roles_pkey");

            entity.ToTable("user_roles");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("user_roles_role_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_roles_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}


