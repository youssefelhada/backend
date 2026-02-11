using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visionguard.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkerEntity_RemoveDepartmentAndIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Workers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Workers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Workers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
