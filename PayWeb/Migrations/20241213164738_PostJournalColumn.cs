using Microsoft.EntityFrameworkCore.Migrations;

namespace CRM.Migrations
{
    public partial class PostJournalColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PostJournal",
                schema: "Finansii",
                table: "BankStatement",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostJournal",
                schema: "Finansii",
                table: "BankStatement");
        }
    }
}
