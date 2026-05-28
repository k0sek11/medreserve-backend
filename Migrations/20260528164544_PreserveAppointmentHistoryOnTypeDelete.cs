using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    /// <inheritdoc />
    public partial class PreserveAppointmentHistoryOnTypeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_appointments_appointment_types_appointment_type_id",
                table: "appointments");

            migrationBuilder.AlterColumn<int>(
                name: "appointment_type_id",
                table: "appointments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "appointment_type_duration_minutes",
                table: "appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE appointments AS a SET appointment_type_duration_minutes = at.duration_minutes FROM appointment_types AS at WHERE a.appointment_type_id = at.appointment_type_id;"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_appointments_appointment_types_appointment_type_id",
                table: "appointments",
                column: "appointment_type_id",
                principalTable: "appointment_types",
                principalColumn: "appointment_type_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_appointments_appointment_types_appointment_type_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "appointment_type_duration_minutes",
                table: "appointments");

            migrationBuilder.AlterColumn<int>(
                name: "appointment_type_id",
                table: "appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_appointments_appointment_types_appointment_type_id",
                table: "appointments",
                column: "appointment_type_id",
                principalTable: "appointment_types",
                principalColumn: "appointment_type_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
