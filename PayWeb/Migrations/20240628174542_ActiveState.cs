using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class ActiveState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActiveState",
                schema: "Finansii",
                table: "BankConfiguration",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveState",
                schema: "Finansii",
                table: "BankConfiguration");
        }
    }
}
