using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class UserHomologationModifKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersHomologation",
                schema: "Finansii",
                table: "UsersHomologation");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyCode",
                schema: "Finansii",
                table: "UsersHomologation",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersHomologation",
                schema: "Finansii",
                table: "UsersHomologation",
                columns: new[] { "PersonalCode", "CompanyCode" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersHomologation",
                schema: "Finansii",
                table: "UsersHomologation");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyCode",
                schema: "Finansii",
                table: "UsersHomologation",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersHomologation",
                schema: "Finansii",
                table: "UsersHomologation",
                column: "PersonalCode");
        }
    }
}
