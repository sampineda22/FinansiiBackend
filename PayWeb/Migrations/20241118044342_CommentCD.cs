using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class CommentCD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                schema: "Finansii",
                table: "CertificatesDeposit",
                type: "varchar(MAX)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                schema: "Finansii",
                table: "CertificatesDeposit");
        }
    }
}
