using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class ModifCDTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                schema: "Finansii",
                table: "CertificatesDeposit");

            migrationBuilder.RenameColumn(
                name: "Start",
                schema: "Finansii",
                table: "CertificatesDeposit",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "End",
                schema: "Finansii",
                table: "CertificatesDeposit",
                newName: "EndDate");

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountInCurrency",
                schema: "Finansii",
                table: "WeeklyRecords",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                schema: "Finansii",
                table: "WeeklyRecords",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal");

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyIncome",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Bank",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal");

            migrationBuilder.AddColumn<decimal>(
                name: "RatePercentage",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatePercentage",
                schema: "Finansii",
                table: "CertificatesDeposit");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                schema: "Finansii",
                table: "CertificatesDeposit",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                schema: "Finansii",
                table: "CertificatesDeposit",
                newName: "End");

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountInCurrency",
                schema: "Finansii",
                table: "WeeklyRecords",
                type: "decimal",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                schema: "Finansii",
                table: "WeeklyRecords",
                type: "decimal",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyIncome",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "decimal",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "Currency",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Bank",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "decimal",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "decimal",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
