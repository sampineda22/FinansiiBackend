using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class ModifySchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "User",
                schema: "Finansii.Admin",
                newName: "User",
                newSchema: "Finansii");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "Finansii.Admin",
                newName: "Roles",
                newSchema: "Finansii");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Finansii.Admin");

            migrationBuilder.RenameTable(
                name: "User",
                schema: "Finansii",
                newName: "User",
                newSchema: "Finansii.Admin");

            migrationBuilder.RenameTable(
                name: "Roles",
                schema: "Finansii",
                newName: "Roles",
                newSchema: "Finansii.Admin");
        }
    }
}
