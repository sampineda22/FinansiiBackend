using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class CompanyCodeKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDates",
                schema: "Finansii",
                table: "PaymentDates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDates",
                schema: "Finansii",
                table: "PaymentDates",
                columns: new[] { "Month", "Year", "CompanyCode" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDates",
                schema: "Finansii",
                table: "PaymentDates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDates",
                schema: "Finansii",
                table: "PaymentDates",
                columns: new[] { "Month", "Year" });
        }
    }
}
