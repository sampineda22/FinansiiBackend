using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class AdminRolesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Admin");

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Admin",
                columns: table => new
                {
                    CompanyCode = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreationUser = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateUser = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => new { x.RoleId, x.CompanyCode });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Admin");
        }
    }
}
