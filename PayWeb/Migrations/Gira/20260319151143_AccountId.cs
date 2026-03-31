using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class AccountId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                schema: "Gira",
                table: "ExpenseCostCenter",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountId",
                schema: "Gira",
                table: "ExpenseCostCenter");
        }
    }
}
