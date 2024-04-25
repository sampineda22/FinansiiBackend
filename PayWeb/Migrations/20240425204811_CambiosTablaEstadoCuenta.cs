using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class CambiosTablaEstadoCuenta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "BankStatementDetails",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "BankStatement",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "BankStatement",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "BankStatementDetails");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "BankStatement");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "BankStatement");
        }
    }
}
