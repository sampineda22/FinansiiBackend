using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class CompanyCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                schema: "Gira",
                table: "ExpenseCostCenter",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                schema: "Gira",
                table: "CostCenterAccount",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyCode",
                schema: "Gira",
                table: "ExpenseCostCenter");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                schema: "Gira",
                table: "CostCenterAccount");
        }
    }
}
