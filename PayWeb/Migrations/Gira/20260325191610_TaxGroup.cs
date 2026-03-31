using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations.Gira
{
    public partial class TaxGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxGroup",
                schema: "Gira",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrupoImpuestoGravado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GrupoImpuestoArticuloGravado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GrupoImpuestoExento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GrupoImpuestoArticuloExento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxGroup", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxGroup",
                schema: "Gira");
        }
    }
}
