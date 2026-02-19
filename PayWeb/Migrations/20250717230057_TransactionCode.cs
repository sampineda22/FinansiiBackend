using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class TransactionCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionCodes",
                schema: "Finansii");

            migrationBuilder.AddColumn<int>(
                name: "TransactionType",
                schema: "Finansii",
                table: "ExceptionCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionType",
                schema: "Finansii",
                table: "ExceptionCodes");

            migrationBuilder.CreateTable(
                name: "TransactionCodes",
                schema: "Finansii",
                columns: table => new
                {
                    BankAccountId = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCodes", x => new { x.BankAccountId, x.Code });
                });
        }
    }
}
