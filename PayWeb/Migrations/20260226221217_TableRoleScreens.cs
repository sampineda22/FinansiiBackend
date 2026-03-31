using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class TableRoleScreens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleScreens",
                schema: "Finansii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleCode = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    ScreenCode = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleScreens", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleScreens",
                schema: "Finansii");
        }
    }
}
