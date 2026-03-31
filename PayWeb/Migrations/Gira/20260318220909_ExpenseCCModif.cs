using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class ExpenseCCModif : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCostCenter_CostCenterAccount_IdCostCenterAccount",
                schema: "Gira",
                table: "ExpenseCostCenter");

            migrationBuilder.DropTable(
                name: "CostCenterAccount",
                schema: "Gira");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCostCenter_IdCostCenterAccount",
                schema: "Gira",
                table: "ExpenseCostCenter");

            migrationBuilder.DropColumn(
                name: "IdCostCenterAccount",
                schema: "Gira",
                table: "ExpenseCostCenter");

            migrationBuilder.AddColumn<string>(
                name: "CostCenterCode",
                schema: "Gira",
                table: "ExpenseCostCenter",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostCenterCode",
                schema: "Gira",
                table: "ExpenseCostCenter");

            migrationBuilder.AddColumn<int>(
                name: "IdCostCenterAccount",
                schema: "Gira",
                table: "ExpenseCostCenter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CostCenterAccount",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Account = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    CostCenterCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenterAccount", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCostCenter_IdCostCenterAccount",
                schema: "Gira",
                table: "ExpenseCostCenter",
                column: "IdCostCenterAccount");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCostCenter_CostCenterAccount_IdCostCenterAccount",
                schema: "Gira",
                table: "ExpenseCostCenter",
                column: "IdCostCenterAccount",
                principalSchema: "Gira",
                principalTable: "CostCenterAccount",
                principalColumn: "Id");
        }
    }
}
