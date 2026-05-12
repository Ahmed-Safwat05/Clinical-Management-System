using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class appuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[] { "Id", "CreatedAt", "DisplayName", "IsActive", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 5, 9, 14, 0, 0, 0, DateTimeKind.Utc), "Administrator", true, "AQAAAAIAAYagAAAAEJpYqKZe0w/9fORDXfzmdCye08Aza/6Hp9g7rjIxm/N2G8nQAqgKvqmOAx6Y5HMQeg==", 0, "admin" },
                    { 2, new DateTime(2025, 5, 9, 14, 0, 0, 0, DateTimeKind.Utc), "Receptionist", true, "AQAAAAIAAYagAAAAEK9a0pFwlqcLo4NzIyvjfEcp6R5zC1bW51vTPDcsddvZbjdQ9XS1f6hINMY3RlOCOA==", 1, "reception" }
                });
        }
    }
}
