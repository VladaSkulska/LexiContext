using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiContext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContextReadingToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Decks",
                newName: "ProficiencyLevel");

            migrationBuilder.AddColumn<string>(
                name: "ContextReading",
                table: "Cards",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContextReading",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "ProficiencyLevel",
                table: "Decks",
                newName: "Level");
        }
    }
}
