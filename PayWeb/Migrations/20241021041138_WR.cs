using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class WR : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "CDWeeklyRecords",
                schema: "Finansii",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificateId = table.Column<int>(type: "int", nullable: false),
                    AmountInCurrency = table.Column<decimal>(type: "decimal", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal", nullable: false),
                    Week = table.Column<int>(type: "int", nullable: false),
                    Journal = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CDWeeklyRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CDWeeklyRecords_CertificatesDeposit_CertificateId",
                        column: x => x.CertificateId,
                        principalSchema: "Finansii",
                        principalTable: "CertificatesDeposit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CDWeeklyRecords_CertificateId",
                schema: "Finansii",
                table: "CDWeeklyRecords",
                column: "CertificateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CDWeeklyRecords",
                schema: "Finansii");

            migrationBuilder.CreateTable(
                name: "WeeklyRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountInCurrency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CertificateId = table.Column<int>(type: "int", nullable: false),
                    Journal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Week = table.Column<int>(type: "int", nullable: false)
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
    }
}
