using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Medreserve.Migrations
{
    /// <inheritdoc />
    public partial class addCitites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "cities",
                columns: new[] { "city_id", "district", "name", "voivodeship" },
                values: new object[,]
                {
                    { 1, "Śródmieście", "Warszawa", "Mazowieckie" },
                    { 2, "Stare Miasto", "Kraków", "Małopolskie" },
                    { 3, "Śródmieście", "Łódź", "Łódzkie" },
                    { 4, "Stare Miasto", "Wrocław", "Dolnośląskie" },
                    { 5, "Stare Miasto", "Poznań", "Wielkopolskie" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "cities",
                keyColumn: "city_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "cities",
                keyColumn: "city_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "cities",
                keyColumn: "city_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "cities",
                keyColumn: "city_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "cities",
                keyColumn: "city_id",
                keyValue: 5);
        }
    }
}
