using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class TableUserRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleCode",
                schema: "Finansii",
                table: "Roles",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "Finansii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    RoleCode = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "Finansii");

            migrationBuilder.DropColumn(
                name: "RoleCode",
                schema: "Finansii",
                table: "Roles");
        }
    }
}
