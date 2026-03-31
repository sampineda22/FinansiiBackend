using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class TableScreenModif : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                schema: "Finansii",
                table: "Screens",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                schema: "Finansii",
                table: "Screens");
        }
    }
}
