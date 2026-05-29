using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class RemoveTaxGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxGroup",
                schema: "Gira",
                table: "ExpenseCategory");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TaxGroup",
                schema: "Gira",
                table: "ExpenseCategory",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
