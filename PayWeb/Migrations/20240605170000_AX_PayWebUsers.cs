using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class AX_PayWebUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AX_PayWebUsers",
                schema: "Finansii",
                columns: table => new
                {
                    PersonalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AXCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AX_PayWebUsers", x => new { x.PersonalCode, x.AXCode });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AX_PayWebUsers",
                schema: "Finansii");
        }
    }
}
