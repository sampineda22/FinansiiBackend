using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class isCapitalizableCD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isCapitalizable",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isCapitalizable",
                schema: "Finansii",
                table: "CertificatesDeposit");
        }
    }
}
