using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class PaymentDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentDates",
                schema: "Finansii",
                columns: table => new
                {
                    Month = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    EndDate = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDates", x => x.Month);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentDates",
                schema: "Finansii");
        }
    }
}
