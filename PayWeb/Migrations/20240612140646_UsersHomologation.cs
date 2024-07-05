using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class UsersHomologation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AX_PayWebUsers",
                schema: "Finansii");

            migrationBuilder.CreateTable(
                name: "UsersHomologation",
                schema: "Finansii",
                columns: table => new
                {
                    PersonalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AXCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    CRMCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersHomologation", x => x.PersonalCode);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsersHomologation",
                schema: "Finansii");

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
    }
}
