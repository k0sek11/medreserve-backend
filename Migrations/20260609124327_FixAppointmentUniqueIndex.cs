using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    public partial class FixAppointmentUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_appointments_doctor_id_appointment_date_start_time",
                table: "appointments");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_doctor_id_appointment_date_start_time",
                table: "appointments",
                columns: new[] { "doctor_id", "appointment_date", "start_time" },
                unique: true,
                filter: "status NOT IN ('Cancelled', 'Completed', 'Unpaid')");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_appointments_doctor_id_appointment_date_start_time",
                table: "appointments");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_doctor_id_appointment_date_start_time",
                table: "appointments",
                columns: new[] { "doctor_id", "appointment_date", "start_time" },
                unique: true);
        }
    }
}
