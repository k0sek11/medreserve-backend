using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Medreserve.Migrations
{
    /// <inheritdoc />
    public partial class SeedSpecializations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "specializations",
                columns: new[] { "specialization_id", "description", "name" },
                values: new object[,]
                {
                    { 1, null, "Alergolog" },
                    { 2, null, "Anestezjolog" },
                    { 3, null, "Chirurg ogólny" },
                    { 4, null, "Internista" },
                    { 5, null, "Dermatolog" },
                    { 6, null, "Diabetolog" },
                    { 7, null, "Endokrynolog" },
                    { 8, null, "Gastroenterolog" },
                    { 9, null, "Ginekolog" },
                    { 10, null, "Kardiolog" },
                    { 11, null, "Lekarz medycyny pracy" },
                    { 12, null, "Lekarz medycyny rodzinnej" },
                    { 13, null, "Neurolog" },
                    { 14, null, "Okulista" },
                    { 15, null, "Onkolog" },
                    { 16, null, "Ortopeda" },
                    { 17, null, "Pediatra" },
                    { 18, null, "Psychiatra" },
                    { 19, null, "Pulmonolog" },
                    { 20, null, "Urolog" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "specializations",
                keyColumn: "specialization_id",
                keyValue: 20);
        }
    }
}
