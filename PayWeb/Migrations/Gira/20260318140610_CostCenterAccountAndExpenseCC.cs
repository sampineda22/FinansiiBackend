using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class CostCenterAccountAndExpenseCC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostCenterAccount",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CostCenterCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Account = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenterAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCostCenter",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCostCenterAccount = table.Column<int>(type: "int", nullable: false),
                    IdExpenseType = table.Column<int>(type: "int", nullable: false),
                    IdExpenseCategory = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCostCenter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseCostCenter_CostCenterAccount_IdCostCenterAccount",
                        column: x => x.IdCostCenterAccount,
                        principalSchema: "Gira",
                        principalTable: "CostCenterAccount",
                        principalColumn: "Id");
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
                name: "IX_ExpenseCostCenter_IdCostCenterAccount",
                schema: "Gira",
                table: "ExpenseCostCenter",
                column: "IdCostCenterAccount");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseCostCenter",
                schema: "Gira");

            migrationBuilder.DropTable(
                name: "CostCenterAccount",
                schema: "Gira");
        }
    }
}
