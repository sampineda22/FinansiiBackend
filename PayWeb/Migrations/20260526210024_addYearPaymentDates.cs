using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class addYearPaymentDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDates",
                schema: "Finansii",
                table: "PaymentDates");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                schema: "Finansii",
                table: "PaymentDates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDates",
                schema: "Finansii",
                table: "PaymentDates",
                columns: new[] { "Month", "Year" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDates",
                schema: "Finansii",
                table: "PaymentDates");

            migrationBuilder.DropColumn(
                name: "Year",
                schema: "Finansii",
                table: "PaymentDates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDates",
                schema: "Finansii",
                table: "PaymentDates",
                column: "Month");
        }
    }
}
