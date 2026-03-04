using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiContext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImplementOAuthLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "ExternalProviderId");

            migrationBuilder.AddColumn<string>(
                name: "AuthProvider",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "AuthProvider", "CreatedAt", "ExternalProviderId", "UpdatedAt" },
                values: new object[] { "System", new DateTime(2026, 3, 3, 9, 25, 41, 579, DateTimeKind.Utc).AddTicks(9868), "sys-001", new DateTime(2026, 3, 3, 9, 25, 41, 579, DateTimeKind.Utc).AddTicks(9870) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthProvider",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ExternalProviderId",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 2, 13, 22, 13, 167, DateTimeKind.Utc).AddTicks(6051), "dummy_hash", new DateTime(2026, 3, 2, 13, 22, 13, 167, DateTimeKind.Utc).AddTicks(6053) });
        }
    }
}
