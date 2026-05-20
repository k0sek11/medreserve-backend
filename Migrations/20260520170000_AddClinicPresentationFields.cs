using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    public partial class AddClinicPresentationFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "clinics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "opening_hours",
                table: "clinics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "map_location",
                table: "clinics",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "clinics");

            migrationBuilder.DropColumn(
                name: "opening_hours",
                table: "clinics");

            migrationBuilder.DropColumn(
                name: "map_location",
                table: "clinics");
        }
    }
}
