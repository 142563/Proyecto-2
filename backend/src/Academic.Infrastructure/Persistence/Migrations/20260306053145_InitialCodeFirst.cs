using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Academic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCodeFirst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "campuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    address = table.Column<string>(type: "character varying(280)", maxLength: 280, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    campus_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'Campus'::character varying"),
                    region = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("campuses_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "programs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("programs_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shifts",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("shifts_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    program_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    credits = table.Column<short>(type: "smallint", nullable: false),
                    cycle = table.Column<short>(type: "smallint", nullable: false),
                    hours_per_week = table.Column<short>(type: "smallint", nullable: false),
                    hours_total = table.Column<short>(type: "smallint", nullable: false),
                    is_lab = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("courses_pkey", x => x.id);
                    table.ForeignKey(
                        name: "courses_program_id_fkey",
                        column: x => x.program_id,
                        principalTable: "programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pricing_catalog",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    program_id = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValueSql: "'GTQ'::character varying"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pricing_catalog_pkey", x => x.id);
                    table.ForeignKey(
                        name: "pricing_catalog_program_id_fkey",
                        column: x => x.program_id,
                        principalTable: "programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "campus_shift_capacity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    campus_id = table.Column<int>(type: "integer", nullable: false),
                    shift_id = table.Column<short>(type: "smallint", nullable: false),
                    total_capacity = table.Column<int>(type: "integer", nullable: false),
                    occupied_capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("campus_shift_capacity_pkey", x => x.id);
                    table.ForeignKey(
                        name: "campus_shift_capacity_campus_id_fkey",
                        column: x => x.campus_id,
                        principalTable: "campuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "campus_shift_capacity_shift_id_fkey",
                        column: x => x.shift_id,
                        principalTable: "shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carnet_prefix_catalog",
                columns: table => new
                {
                    prefix = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    program_id = table.Column<int>(type: "integer", nullable: false),
                    campus_id = table.Column<int>(type: "integer", nullable: false),
                    shift_id = table.Column<short>(type: "smallint", nullable: false),
                    description = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("carnet_prefix_catalog_pkey", x => x.prefix);
                    table.ForeignKey(
                        name: "carnet_prefix_catalog_campus_id_fkey",
                        column: x => x.campus_id,
                        principalTable: "campuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "carnet_prefix_catalog_program_id_fkey",
                        column: x => x.program_id,
                        principalTable: "programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "carnet_prefix_catalog_shift_id_fkey",
                        column: x => x.shift_id,
                        principalTable: "shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_logs_pkey", x => x.id);
                    table.ForeignKey(
                        name: "audit_logs_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_roles_pkey", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "user_roles_role_id_fkey",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "user_roles_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_credit_requirements",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    min_approved_credits = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("course_credit_requirements_pkey", x => x.id);
                    table.ForeignKey(
                        name: "course_credit_requirements_course_id_fkey",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_prerequisites",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    prerequisite_course_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("course_prerequisites_pkey", x => new { x.course_id, x.prerequisite_course_id });
                    table.ForeignKey(
                        name: "course_prerequisites_course_id_fkey",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "course_prerequisites_prerequisite_course_id_fkey",
                        column: x => x.prerequisite_course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    institutional_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    last_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    carnet = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    carnet_prefix = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    entry_year = table.Column<short>(type: "smallint", nullable: false),
                    carnet_sequence = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    program_id = table.Column<int>(type: "integer", nullable: false),
                    current_campus_id = table.Column<int>(type: "integer", nullable: true),
                    current_shift_id = table.Column<short>(type: "smallint", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("students_pkey", x => x.id);
                    table.ForeignKey(
                        name: "students_carnet_prefix_fkey",
                        column: x => x.carnet_prefix,
                        principalTable: "carnet_prefix_catalog",
                        principalColumn: "prefix",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "students_current_campus_id_fkey",
                        column: x => x.current_campus_id,
                        principalTable: "campuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "students_current_shift_id_fkey",
                        column: x => x.current_shift_id,
                        principalTable: "shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "students_program_id_fkey",
                        column: x => x.program_id,
                        principalTable: "programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "students_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("enrollments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "enrollments_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValueSql: "'GTQ'::character varying"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(now() + '72:00:00'::interval)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("payment_orders_pkey", x => x.id);
                    table.ForeignKey(
                        name: "payment_orders_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_course_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<short>(type: "smallint", nullable: false),
                    term = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    grade = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("student_course_history_pkey", x => x.id);
                    table.ForeignKey(
                        name: "student_course_history_course_id_fkey",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "student_course_history_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transfer_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_campus_id = table.Column<int>(type: "integer", nullable: true),
                    to_campus_id = table.Column<int>(type: "integer", nullable: false),
                    to_shift_id = table.Column<short>(type: "smallint", nullable: false),
                    modality = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    reviewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    review_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("transfer_requests_pkey", x => x.id);
                    table.ForeignKey(
                        name: "transfer_requests_from_campus_id_fkey",
                        column: x => x.from_campus_id,
                        principalTable: "campuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "transfer_requests_reviewed_by_user_id_fkey",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "transfer_requests_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "transfer_requests_to_campus_id_fkey",
                        column: x => x.to_campus_id,
                        principalTable: "campuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "transfer_requests_to_shift_id_fkey",
                        column: x => x.to_shift_id,
                        principalTable: "shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_courses",
                columns: table => new
                {
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    is_overdue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("enrollment_courses_pkey", x => new { x.enrollment_id, x.course_id });
                    table.ForeignKey(
                        name: "enrollment_courses_course_id_fkey",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "enrollment_courses_enrollment_id_fkey",
                        column: x => x.enrollment_id,
                        principalTable: "enrollments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "certificates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purpose = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    verification_code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    pdf_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("certificates_pkey", x => x.id);
                    table.ForeignKey(
                        name: "certificates_payment_order_id_fkey",
                        column: x => x.payment_order_id,
                        principalTable: "payment_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "certificates_student_id_fkey",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_entity_created",
                table: "audit_logs",
                columns: new[] { "entity_name", "entity_id", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_user_created",
                table: "audit_logs",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_campus_shift_capacity_shift_id",
                table: "campus_shift_capacity",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "uq_campus_shift",
                table: "campus_shift_capacity",
                columns: new[] { "campus_id", "shift_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "campuses_code_key",
                table: "campuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "campuses_name_key",
                table: "campuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_campuses_type_region",
                table: "campuses",
                columns: new[] { "campus_type", "region" });

            migrationBuilder.CreateIndex(
                name: "idx_carnet_prefix_campus_id",
                table: "carnet_prefix_catalog",
                column: "campus_id");

            migrationBuilder.CreateIndex(
                name: "idx_carnet_prefix_program_id",
                table: "carnet_prefix_catalog",
                column: "program_id");

            migrationBuilder.CreateIndex(
                name: "idx_carnet_prefix_shift_id",
                table: "carnet_prefix_catalog",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "certificates_payment_order_id_key",
                table: "certificates",
                column: "payment_order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "certificates_verification_code_key",
                table: "certificates",
                column: "verification_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_certificates_code",
                table: "certificates",
                column: "verification_code");

            migrationBuilder.CreateIndex(
                name: "idx_certificates_student_status",
                table: "certificates",
                columns: new[] { "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "uq_certificate_active_per_student",
                table: "certificates",
                column: "student_id",
                unique: true,
                filter: "((status)::text = ANY ((ARRAY['Requested'::character varying, 'PdfGenerated'::character varying])::text[]))");

            migrationBuilder.CreateIndex(
                name: "idx_course_credit_requirements_course_id",
                table: "course_credit_requirements",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_prerequisites_prerequisite_course_id",
                table: "course_prerequisites",
                column: "prerequisite_course_id");

            migrationBuilder.CreateIndex(
                name: "courses_code_key",
                table: "courses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_courses_program_cycle",
                table: "courses",
                columns: new[] { "program_id", "cycle" });

            migrationBuilder.CreateIndex(
                name: "idx_courses_program_id",
                table: "courses",
                column: "program_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_courses_course_id",
                table: "enrollment_courses",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "idx_enrollments_student_status",
                table: "enrollments",
                columns: new[] { "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "uq_enrollment_active_per_student",
                table: "enrollments",
                column: "student_id",
                unique: true,
                filter: "((status)::text = 'PendingPayment'::text)");

            migrationBuilder.CreateIndex(
                name: "idx_payment_orders_reference",
                table: "payment_orders",
                columns: new[] { "reference_id", "order_type" });

            migrationBuilder.CreateIndex(
                name: "idx_payment_orders_status_expires",
                table: "payment_orders",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "idx_payment_orders_student_status",
                table: "payment_orders",
                columns: new[] { "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "uq_payment_reference",
                table: "payment_orders",
                columns: new[] { "order_type", "reference_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pricing_catalog_program_id",
                table: "pricing_catalog",
                column: "program_id");

            migrationBuilder.CreateIndex(
                name: "programs_code_key",
                table: "programs",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "roles_name_key",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "shifts_name_key",
                table: "shifts",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_history_course_student",
                table: "student_course_history",
                columns: new[] { "course_id", "student_id" });

            migrationBuilder.CreateIndex(
                name: "idx_history_student_status",
                table: "student_course_history",
                columns: new[] { "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_students_program_id",
                table: "students",
                column: "program_id");

            migrationBuilder.CreateIndex(
                name: "idx_students_user_id",
                table: "students",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_carnet_prefix",
                table: "students",
                column: "carnet_prefix");

            migrationBuilder.CreateIndex(
                name: "IX_students_current_campus_id",
                table: "students",
                column: "current_campus_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_current_shift_id",
                table: "students",
                column: "current_shift_id");

            migrationBuilder.CreateIndex(
                name: "students_carnet_key",
                table: "students",
                column: "carnet",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "students_institutional_email_key",
                table: "students",
                column: "institutional_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "students_student_code_key",
                table: "students",
                column: "student_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "students_user_id_key",
                table: "students",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_transfers_destination",
                table: "transfer_requests",
                columns: new[] { "to_campus_id", "to_shift_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_transfers_student_status",
                table: "transfer_requests",
                columns: new[] { "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_from_campus_id",
                table: "transfer_requests",
                column: "from_campus_id");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_reviewed_by_user_id",
                table: "transfer_requests",
                column: "reviewed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_to_shift_id",
                table: "transfer_requests",
                column: "to_shift_id");

            migrationBuilder.CreateIndex(
                name: "uq_transfer_active_per_student",
                table: "transfer_requests",
                column: "student_id",
                unique: true,
                filter: "((status)::text = ANY ((ARRAY['PendingPayment'::character varying, 'PendingReview'::character varying])::text[]))");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "users_email_key",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "campus_shift_capacity");

            migrationBuilder.DropTable(
                name: "certificates");

            migrationBuilder.DropTable(
                name: "course_credit_requirements");

            migrationBuilder.DropTable(
                name: "course_prerequisites");

            migrationBuilder.DropTable(
                name: "enrollment_courses");

            migrationBuilder.DropTable(
                name: "pricing_catalog");

            migrationBuilder.DropTable(
                name: "student_course_history");

            migrationBuilder.DropTable(
                name: "transfer_requests");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "payment_orders");

            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "carnet_prefix_catalog");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "campuses");

            migrationBuilder.DropTable(
                name: "programs");

            migrationBuilder.DropTable(
                name: "shifts");
        }
    }
}
