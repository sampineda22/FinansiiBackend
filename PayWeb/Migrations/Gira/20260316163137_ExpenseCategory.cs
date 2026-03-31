using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class ExpenseCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpenseCategory",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdExpenseType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsInvoiceRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsDescriptionRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsImageRequired = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    VendAccount = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TaxGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseCategory_ExpensesType_IdExpenseType",
                        column: x => x.IdExpenseType,
                        principalSchema: "Gira",
                        principalTable: "ExpensesType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategory_IdExpenseType",
                schema: "Gira",
                table: "ExpenseCategory",
                column: "IdExpenseType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseCategory",
                schema: "Gira");
        }
    }
}
