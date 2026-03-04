using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiContext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CurrentStreak", "Email", "LastStudyDate", "PasswordHash", "UpdatedAt", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 3, 2, 13, 22, 13, 167, DateTimeKind.Utc).AddTicks(6051), 0, "student@fpm.kpi.ua", null, "dummy_hash", new DateTime(2026, 3, 2, 13, 22, 13, 167, DateTimeKind.Utc).AddTicks(6053), "DevStudent_KPI" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));
        }
    }
}
