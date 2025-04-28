using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class ExceptionaAndEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Finansii",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                schema: "Finansii",
                table: "Emails");

            migrationBuilder.AddColumn<string>(
                name: "ProjectCode",
                schema: "Finansii",
                table: "Emails",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ExceptionCodes",
                schema: "Finansii",
                columns: table => new
                {
                    AccountId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionCodes", x => new { x.AccountId, x.Code });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExceptionCodes",
                schema: "Finansii");

            migrationBuilder.DropColumn(
                name: "ProjectCode",
                schema: "Finansii",
                table: "Emails");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Finansii",
                table: "Emails",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                schema: "Finansii",
                table: "Emails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
