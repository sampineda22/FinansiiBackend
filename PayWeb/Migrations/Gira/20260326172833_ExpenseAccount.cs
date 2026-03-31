using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class ExpenseAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseCostCenter",
                schema: "Gira");

            migrationBuilder.CreateTable(
                name: "ExpenseAccount",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    IdExpenseType = table.Column<int>(type: "int", nullable: false),
                    IdExpenseCategory = table.Column<int>(type: "int", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseAccount_ExpenseCategory_IdExpenseCategory",
                        column: x => x.IdExpenseCategory,
                        principalSchema: "Gira",
                        principalTable: "ExpenseCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExpenseAccount_ExpensesType_IdExpenseType",
                        column: x => x.IdExpenseType,
                        principalSchema: "Gira",
                        principalTable: "ExpensesType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAccount_IdExpenseCategory",
                schema: "Gira",
                table: "ExpenseAccount",
                column: "IdExpenseCategory");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAccount_IdExpenseType",
                schema: "Gira",
                table: "ExpenseAccount",
                column: "IdExpenseType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseAccount",
                schema: "Gira");

            migrationBuilder.CreateTable(
                name: "ExpenseCostCenter",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    CostCenterCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdExpenseCategory = table.Column<int>(type: "int", nullable: false),
                    IdExpenseType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCostCenter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseCostCenter_ExpenseCategory_IdExpenseCategory",
                        column: x => x.IdExpenseCategory,
                        principalSchema: "Gira",
                        principalTable: "ExpenseCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExpenseCostCenter_ExpensesType_IdExpenseType",
                        column: x => x.IdExpenseType,
                        principalSchema: "Gira",
                        principalTable: "ExpensesType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCostCenter_IdExpenseCategory",
                schema: "Gira",
                table: "ExpenseCostCenter",
                column: "IdExpenseCategory");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCostCenter_IdExpenseType",
                schema: "Gira",
                table: "ExpenseCostCenter",
                column: "IdExpenseType");
        }
    }
}
