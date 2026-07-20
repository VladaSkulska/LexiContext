using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiContext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerClassroomIdToDeck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerClassroomId",
                table: "Decks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentHomeworks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskText = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentHomeworks", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 13, 15, 56, 40, 477, DateTimeKind.Utc).AddTicks(5144), new DateTime(2026, 7, 13, 15, 56, 40, 477, DateTimeKind.Utc).AddTicks(5144) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentHomeworks");

            migrationBuilder.DropColumn(
                name: "OwnerClassroomId",
                table: "Decks");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 29, 18, 36, 46, 438, DateTimeKind.Utc).AddTicks(7072), new DateTime(2026, 6, 29, 18, 36, 46, 438, DateTimeKind.Utc).AddTicks(7073) });
        }
    }
}
