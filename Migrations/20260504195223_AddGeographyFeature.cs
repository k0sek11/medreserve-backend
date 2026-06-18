using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Medreserve.Migrations
{
    public partial class AddGeographyFeature : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "address",
                table: "clinics",
                newName: "street_address");

            migrationBuilder.AddColumn<int>(
                name: "city_id",
                table: "clinics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    city_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    district = table.Column<string>(type: "text", nullable: false),
                    voivodeship = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cities", x => x.city_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clinics_city_id",
                table: "clinics",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_cities_name_district_voivodeship",
                table: "cities",
                columns: new[] { "name", "district", "voivodeship" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_clinics_cities_city_id",
                table: "clinics",
                column: "city_id",
                principalTable: "cities",
                principalColumn: "city_id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_clinics_cities_city_id",
                table: "clinics");

            migrationBuilder.DropTable(
                name: "cities");

            migrationBuilder.DropIndex(
                name: "ix_clinics_city_id",
                table: "clinics");

            migrationBuilder.DropColumn(
                name: "city_id",
                table: "clinics");

            migrationBuilder.RenameColumn(
                name: "street_address",
                table: "clinics",
                newName: "address");
        }
    }
}
