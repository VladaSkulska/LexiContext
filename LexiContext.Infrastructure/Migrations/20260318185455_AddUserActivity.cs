using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiContext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Users_CreaterId",
                table: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_Decks_CreaterId",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "CreaterId",
                table: "Decks");

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CardsStudied = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 18, 18, 54, 54, 928, DateTimeKind.Utc).AddTicks(7750), new DateTime(2026, 3, 18, 18, 54, 54, 928, DateTimeKind.Utc).AddTicks(7751) });

            migrationBuilder.CreateIndex(
                name: "IX_Decks_CreatedId",
                table: "Decks",
                column: "CreatedId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_UserId",
                table: "UserActivities",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Users_CreatedId",
                table: "Decks",
                column: "CreatedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Users_CreatedId",
                table: "Decks");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropIndex(
                name: "IX_Decks_CreatedId",
                table: "Decks");

            migrationBuilder.AddColumn<Guid>(
                name: "CreaterId",
                table: "Decks",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 4, 20, 6, 22, 621, DateTimeKind.Utc).AddTicks(9390), new DateTime(2026, 3, 4, 20, 6, 22, 621, DateTimeKind.Utc).AddTicks(9392) });

            migrationBuilder.CreateIndex(
                name: "IX_Decks_CreaterId",
                table: "Decks",
                column: "CreaterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Users_CreaterId",
                table: "Decks",
                column: "CreaterId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
