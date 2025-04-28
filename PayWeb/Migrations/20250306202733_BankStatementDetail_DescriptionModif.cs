using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class BankStatementDetail_DescriptionModif : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Finansii",
                table: "BankStatementDetails",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(125)",
                oldMaxLength: 125);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Finansii",
                table: "BankStatementDetails",
                type: "nvarchar(125)",
                maxLength: 125,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);
        }
    }
}
