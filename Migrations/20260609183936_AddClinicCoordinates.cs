using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "map_location",
                table: "clinics");

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "clinics",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "clinics",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "clinics");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "clinics");

            migrationBuilder.AddColumn<string>(
                name: "map_location",
                table: "clinics",
                type: "text",
                nullable: true);
        }
    }
}
