using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftToEnrollmentCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "shift_id",
                table: "enrollment_courses",
                type: "smallint",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE enrollment_courses ec
                SET shift_id = COALESCE(s.current_shift_id, cpc.shift_id, 1)
                FROM enrollments e
                JOIN students s ON s.id = e.student_id
                LEFT JOIN carnet_prefix_catalog cpc ON cpc.prefix = s.carnet_prefix
                WHERE ec.enrollment_id = e.id;
                """);

            migrationBuilder.Sql("""
                UPDATE enrollment_courses
                SET shift_id = 1
                WHERE shift_id IS NULL;
                """);

            migrationBuilder.AlterColumn<short>(
                name: "shift_id",
                table: "enrollment_courses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_enrollment_courses_shift",
                table: "enrollment_courses",
                columns: new[] { "enrollment_id", "shift_id" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_courses_shift_id",
                table: "enrollment_courses",
                column: "shift_id");

            migrationBuilder.AddForeignKey(
                name: "enrollment_courses_shift_id_fkey",
                table: "enrollment_courses",
                column: "shift_id",
                principalTable: "shifts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "enrollment_courses_shift_id_fkey",
                table: "enrollment_courses");

            migrationBuilder.DropIndex(
                name: "idx_enrollment_courses_shift",
                table: "enrollment_courses");

            migrationBuilder.DropIndex(
                name: "IX_enrollment_courses_shift_id",
                table: "enrollment_courses");

            migrationBuilder.DropColumn(
                name: "shift_id",
                table: "enrollment_courses");
        }
    }
}
