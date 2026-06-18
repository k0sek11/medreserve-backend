using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    public partial class AddClinicToDoctorSchedules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "clinic_id",
                table: "doctor_schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_doctor_schedules_clinic_id",
                table: "doctor_schedules",
                column: "clinic_id");

            migrationBuilder.AddForeignKey(
                name: "fk_doctor_schedules_clinics_clinic_id",
                table: "doctor_schedules",
                column: "clinic_id",
                principalTable: "clinics",
                principalColumn: "clinic_id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_doctor_schedules_clinics_clinic_id",
                table: "doctor_schedules");

            migrationBuilder.DropIndex(
                name: "ix_doctor_schedules_clinic_id",
                table: "doctor_schedules");

            migrationBuilder.DropColumn(
                name: "clinic_id",
                table: "doctor_schedules");
        }
    }
}
