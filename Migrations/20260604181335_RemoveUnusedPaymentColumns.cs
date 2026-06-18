using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medreserve.Migrations
{
    public partial class RemoveUnusedPaymentColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "failure_reason",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "provider",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "provider_transaction_id",
                table: "payments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_transaction_id",
                table: "payments",
                type: "text",
                nullable: true);
        }
    }
}
