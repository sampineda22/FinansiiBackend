using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class Validations3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                schema: "Finansii",
                table: "Validations");

            migrationBuilder.RenameColumn(
                name: "Field",
                schema: "Finansii",
                table: "Validations",
                newName: "ConditionField");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectCode",
                schema: "Finansii",
                table: "Validations",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AddColumn<string>(
                name: "ConditionValue",
                schema: "Finansii",
                table: "Validations",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredFiled",
                schema: "Finansii",
                table: "Validations",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionValue",
                schema: "Finansii",
                table: "Validations");

            migrationBuilder.DropColumn(
                name: "RequiredFiled",
                schema: "Finansii",
                table: "Validations");

            migrationBuilder.RenameColumn(
                name: "ConditionField",
                schema: "Finansii",
                table: "Validations",
                newName: "Field");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectCode",
                schema: "Finansii",
                table: "Validations",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                schema: "Finansii",
                table: "Validations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
