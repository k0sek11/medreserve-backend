using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    public partial class ReplaceAgeWithBirthDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" DROP COLUMN IF EXISTS age;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ADD COLUMN IF NOT EXISTS birth_date date NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" DROP COLUMN IF EXISTS birth_date;");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" ADD COLUMN IF NOT EXISTS age integer NULL;");
        }
    }
}
