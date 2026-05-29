using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class createAgain_ExpenseDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpenseDetails",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpenseCategoryId = table.Column<int>(type: "int", nullable: false),
                    MealId = table.Column<int>(type: "int", nullable: true),
                    FuelTypeId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    PersonalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VendAccount = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    InvoiceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SeriesNum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExemptAmount = table.Column<double>(type: "float", nullable: true),
                    GravadoAmount = table.Column<double>(type: "float", nullable: true),
                    InvoiceAmount = table.Column<double>(type: "float", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "'1900-01-01'"),
                    ImagePath = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "'1900-01-01'"),
                    PersonalCodeAdmin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RejectionMotive = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    JournalNum = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    InUse = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_ExpenseCategory_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalSchema: "Gira",
                        principalTable: "ExpenseCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_FuelTypes_FuelTypeId",
                        column: x => x.FuelTypeId,
                        principalSchema: "Gira",
                        principalTable: "FuelTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_Status_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Gira",
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_ExpenseCategoryId",
                schema: "Gira",
                table: "ExpenseDetails",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_FuelTypeId",
                schema: "Gira",
                table: "ExpenseDetails",
                column: "FuelTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_StatusId",
                schema: "Gira",
                table: "ExpenseDetails",
                column: "StatusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpenseDetails",
                schema: "Gira");
        }
    }
}
