using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class AutoApprovePositionsMap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoApprovePositions",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    PositionCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoApprovePositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoApprovePositions_CompanyCode_CategoryCode_PositionCode",
                schema: "Gira",
                table: "AutoApprovePositions",
                columns: new[] { "CompanyCode", "CategoryCode", "PositionCode" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoApprovePositions",
                schema: "Gira");
        }
    }
}
