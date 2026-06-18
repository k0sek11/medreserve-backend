using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    public partial class MakeAppointmentTypeNamesNonUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_appointment_types_name",
                table: "appointment_types");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_appointment_types_name",
                table: "appointment_types",
                column: "name",
                unique: true);
        }
    }
}
