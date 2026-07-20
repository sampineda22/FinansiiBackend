using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class ExceptionCodes_NewKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExceptionCodes",
                schema: "Finansii",
                table: "ExceptionCodes");

            migrationBuilder.RenameColumn(
                name: "RequiredFiled",
                schema: "Finansii",
                table: "Validations",
                newName: "RequiredField");

            migrationBuilder.AddColumn<string>(
                name: "ConditionType",
                schema: "Finansii",
                table: "Validations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredType",
                schema: "Finansii",
                table: "Validations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredValue",
                schema: "Finansii",
                table: "Validations",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExceptionCodes",
                schema: "Finansii",
                table: "ExceptionCodes",
                columns: new[] { "AccountId", "Code", "TransactionType" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExceptionCodes",
                schema: "Finansii",
                table: "ExceptionCodes");

            migrationBuilder.DropColumn(
                name: "ConditionType",
                schema: "Finansii",
                table: "Validations");

            migrationBuilder.DropColumn(
                name: "RequiredType",
                schema: "Finansii",
                table: "Validations");

            migrationBuilder.DropColumn(
                name: "RequiredValue",
                schema: "Finansii",
                table: "Validations");

            migrationBuilder.RenameColumn(
                name: "RequiredField",
                schema: "Finansii",
                table: "Validations",
                newName: "RequiredFiled");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExceptionCodes",
                schema: "Finansii",
                table: "ExceptionCodes",
                columns: new[] { "AccountId", "Code" });
        }
    }
}
