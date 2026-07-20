using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiContext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureDeckClassroomRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 13, 16, 9, 49, 755, DateTimeKind.Utc).AddTicks(2793), new DateTime(2026, 7, 13, 16, 9, 49, 755, DateTimeKind.Utc).AddTicks(2794) });

            migrationBuilder.CreateIndex(
                name: "IX_Decks_OwnerClassroomId",
                table: "Decks",
                column: "OwnerClassroomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Classrooms_OwnerClassroomId",
                table: "Decks",
                column: "OwnerClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Classrooms_OwnerClassroomId",
                table: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_Decks_OwnerClassroomId",
                table: "Decks");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 13, 15, 56, 40, 477, DateTimeKind.Utc).AddTicks(5144), new DateTime(2026, 7, 13, 15, 56, 40, 477, DateTimeKind.Utc).AddTicks(5144) });
        }
    }
}
