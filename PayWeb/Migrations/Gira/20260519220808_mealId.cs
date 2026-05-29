using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class mealId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpenseNote",
                schema: "Gira",
                table: "ExpenseDetails");

            migrationBuilder.AddColumn<int>(
                name: "MealId",
                schema: "Gira",
                table: "ExpenseDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MealId",
                schema: "Gira",
                table: "ExpenseDetails");

            migrationBuilder.AddColumn<string>(
                name: "ExpenseNote",
                schema: "Gira",
                table: "ExpenseDetails",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
