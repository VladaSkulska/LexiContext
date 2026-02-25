using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiContext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiSettingsToDeck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Box",
                table: "UserCardProgresses",
                newName: "Repetitions");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Decks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Tone",
                table: "Decks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "Tone",
                table: "Decks");

            migrationBuilder.RenameColumn(
                name: "Repetitions",
                table: "UserCardProgresses",
                newName: "Box");
        }
    }
}
