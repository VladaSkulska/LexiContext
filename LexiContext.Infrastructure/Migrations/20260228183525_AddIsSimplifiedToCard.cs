using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiContext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSimplifiedToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSimplified",
                table: "Cards",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSimplified",
                table: "Cards");
        }
    }
}
