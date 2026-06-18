using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    public partial class RemoveCitiesAddCityString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "fk_clinics_cities_city_id",
                table: "clinics");


            migrationBuilder.DropIndex(
                name: "ix_clinics_city_id",
                table: "clinics");


            migrationBuilder.DropColumn(
                name: "city_id",
                table: "clinics");


            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "clinics",
                type: "text",
                nullable: false,
                defaultValue: "");


            migrationBuilder.DropTable(
                name: "cities");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    city_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    district = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    voivodeship = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cities", x => x.city_id);
                });


            migrationBuilder.CreateIndex(
                name: "ix_cities_name_district_voivodeship",
                table: "cities",
                columns: new[] { "name", "district", "voivodeship" },
                unique: true);


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


            migrationBuilder.DropColumn(
                name: "city",
                table: "clinics");


            migrationBuilder.AddColumn<int>(
                name: "city_id",
                table: "clinics",
                type: "integer",
                nullable: false,
                defaultValue: 0);


            migrationBuilder.CreateIndex(
                name: "ix_clinics_city_id",
                table: "clinics",
                column: "city_id");

            migrationBuilder.AddForeignKey(
                name: "fk_clinics_cities_city_id",
                table: "clinics",
                column: "city_id",
                principalTable: "cities",
                principalColumn: "city_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
