using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class PostJournalColumnBC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostJournal",
                schema: "Finansii",
                table: "BankStatement");

            migrationBuilder.AddColumn<bool>(
                name: "PostJournal",
                schema: "Finansii",
                table: "BankConfiguration",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostJournal",
                schema: "Finansii",
                table: "BankConfiguration");

            migrationBuilder.AddColumn<bool>(
                name: "PostJournal",
                schema: "Finansii",
                table: "BankStatement",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
