using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class Status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseDetails_FuelTypes_FuelTypeId",
                schema: "Gira",
                table: "ExpenseDetails");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "Gira",
                table: "ExpenseDetails",
                newName: "StatusId");

            migrationBuilder.AlterColumn<int>(
                name: "FuelTypeId",
                schema: "Gira",
                table: "ExpenseDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Status",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_StatusId",
                schema: "Gira",
                table: "ExpenseDetails",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseDetails_FuelTypes_FuelTypeId",
                schema: "Gira",
                table: "ExpenseDetails",
                column: "FuelTypeId",
                principalSchema: "Gira",
                principalTable: "FuelTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseDetails_Status_StatusId",
                schema: "Gira",
                table: "ExpenseDetails",
                column: "StatusId",
                principalSchema: "Gira",
                principalTable: "Status",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseDetails_FuelTypes_FuelTypeId",
                schema: "Gira",
                table: "ExpenseDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseDetails_Status_StatusId",
                schema: "Gira",
                table: "ExpenseDetails");

            migrationBuilder.DropTable(
                name: "Status",
                schema: "Gira");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseDetails_StatusId",
                schema: "Gira",
                table: "ExpenseDetails");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                schema: "Gira",
                table: "ExpenseDetails",
                newName: "Status");

            migrationBuilder.AlterColumn<int>(
                name: "FuelTypeId",
                schema: "Gira",
                table: "ExpenseDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseDetails_FuelTypes_FuelTypeId",
                schema: "Gira",
                table: "ExpenseDetails",
                column: "FuelTypeId",
                principalSchema: "Gira",
                principalTable: "FuelTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
