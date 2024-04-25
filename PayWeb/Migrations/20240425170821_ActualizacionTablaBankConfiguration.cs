using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class ActualizacionTablaBankConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocalFileRoute",
                table: "BankConfiguration",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalFileRoute",
                table: "BankConfiguration");
        }
    }
}
