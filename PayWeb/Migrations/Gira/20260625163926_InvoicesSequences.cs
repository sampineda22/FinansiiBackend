using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class InvoicesSequences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiraSequences",
                schema: "Gira");

            migrationBuilder.CreateTable(
                name: "InvoicesSequences",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Initials = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicesSequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesSequences_CompanyCode_Initials",
                schema: "Gira",
                table: "InvoicesSequences",
                columns: new[] { "CompanyCode", "Initials" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoicesSequences",
                schema: "Gira");

            migrationBuilder.CreateTable(
                name: "GiraSequences",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Initials = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiraSequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiraSequences_CompanyCode_Initials",
                schema: "Gira",
                table: "GiraSequences",
                columns: new[] { "CompanyCode", "Initials" },
                unique: true);
        }
    }
}
