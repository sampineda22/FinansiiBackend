using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class TablesForCD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankStatementDetails",
                schema: "Finansii");

            migrationBuilder.CreateTable(
                name: "CertificatesDeposit",
                schema: "Finansii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Bank = table.Column<int>(type: "int", nullable: false),
                    CDNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Start = table.Column<DateTime>(type: "date", nullable: false),
                    End = table.Column<DateTime>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal", nullable: false),
                    DailyIncome = table.Column<decimal>(type: "decimal", nullable: false),
                    isEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificatesDeposit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificateId = table.Column<int>(type: "int", nullable: false),
                    AmountInCurrency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Week = table.Column<int>(type: "int", nullable: false),
                    Journal = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyRecord_CertificatesDeposit_CertificateId",
                        column: x => x.CertificateId,
                        principalSchema: "Finansii",
                        principalTable: "CertificatesDeposit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyRecord_CertificateId",
                table: "WeeklyRecord",
                column: "CertificateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeeklyRecord");

            migrationBuilder.DropTable(
                name: "CertificatesDeposit",
                schema: "Finansii");

            migrationBuilder.CreateTable(
                name: "BankStatementDetails",
                schema: "Finansii",
                columns: table => new
                {
                    BankStatementDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BankStatementId = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(125)", maxLength: 125, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(125)", maxLength: 125, nullable: false),
                    TransactionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStatementDetails", x => x.BankStatementDetailId);
                    table.ForeignKey(
                        name: "FK_BankStatementDetails_BankStatement_BankStatementId",
                        column: x => x.BankStatementId,
                        principalSchema: "Finansii",
                        principalTable: "BankStatement",
                        principalColumn: "BankStatementId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementDetails_BankStatementId",
                schema: "Finansii",
                table: "BankStatementDetails",
                column: "BankStatementId");
        }
    }
}
