using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class eschemaAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Finansii");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "User",
                newSchema: "Finansii");

            migrationBuilder.RenameTable(
                name: "BankStatementDetails",
                newName: "BankStatementDetails",
                newSchema: "Finansii");

            migrationBuilder.RenameTable(
                name: "BankStatement",
                newName: "BankStatement",
                newSchema: "Finansii");

            migrationBuilder.RenameTable(
                name: "BankConfiguration",
                newName: "BankConfiguration",
                newSchema: "Finansii");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "User",
                schema: "Finansii",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "BankStatementDetails",
                schema: "Finansii",
                newName: "BankStatementDetails");

            migrationBuilder.RenameTable(
                name: "BankStatement",
                schema: "Finansii",
                newName: "BankStatement");

            migrationBuilder.RenameTable(
                name: "BankConfiguration",
                schema: "Finansii",
                newName: "BankConfiguration");
        }
    }
}
