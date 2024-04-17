using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class AdicionTablaeEstadoCuenta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankStatementDetails",
                columns: table => new
                {
                    BankStatementDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankStatementId = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<int>(type: "int", nullable: false),
                    TransactionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(125)", maxLength: 125, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(125)", maxLength: 125, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementDetails", x => x.BankStatementDetailId);
                    table.ForeignKey(
                        name: "FK_BankStatementDetails_BankStatement_BankStatementId",
                        column: x => x.BankStatementId,
                        principalTable: "BankStatement",
                        principalColumn: "BankStatementId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementDetails_BankStatementId",
                table: "BankStatementDetails",
                column: "BankStatementId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankStatementDetails");
        }
    }
}
