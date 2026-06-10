using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToPrescriptionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PrescriptionItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PrescriptionItems");
        }
    }
}
