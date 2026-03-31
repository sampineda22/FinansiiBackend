using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class TableRolesModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                schema: "Finansii",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                schema: "Finansii",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "UpdateUser",
                schema: "Finansii",
                table: "Roles",
                newName: "ModifiedUser");

            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                schema: "Finansii",
                table: "Roles",
                newName: "ModifiedDate");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Finansii",
                table: "Roles",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                schema: "Finansii",
                table: "Roles",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                schema: "Finansii",
                table: "Roles",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                schema: "Finansii",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "RoleName",
                schema: "Finansii",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "ModifiedUser",
                schema: "Finansii",
                table: "Roles",
                newName: "UpdateUser");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                schema: "Finansii",
                table: "Roles",
                newName: "UpdateDate");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Finansii",
                table: "Roles",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                schema: "Finansii",
                table: "Roles",
                type: "varchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                schema: "Finansii",
                table: "Roles",
                columns: new[] { "RoleId", "CompanyCode" });
        }
    }
}
