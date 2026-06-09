using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTimeSlotIdWithDateAndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_appointments_doctor_id",
                table: "appointments");

            migrationBuilder.DropIndex(
                name: "ix_appointments_time_slot_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "time_slot_id",
                table: "appointments");

            migrationBuilder.AddColumn<DateOnly>(
                name: "appointment_date",
                table: "appointments",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "start_time",
                table: "appointments",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "ix_appointments_doctor_id_appointment_date_start_time",
                table: "appointments",
                columns: new[] { "doctor_id", "appointment_date", "start_time" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_appointments_doctor_id_appointment_date_start_time",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "appointment_date",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "start_time",
                table: "appointments");

            migrationBuilder.AddColumn<int>(
                name: "time_slot_id",
                table: "appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_appointments_doctor_id",
                table: "appointments",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_time_slot_id",
                table: "appointments",
                column: "time_slot_id",
                unique: true);
        }
    }
}
