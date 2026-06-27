using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualPet.Migrations
{
    /// <inheritdoc />
    public partial class AddWindowHealthPenaltyTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HealthPenaltyAppliedInWindow",
                table: "Pets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HealthPenaltyAppliedInWindow",
                table: "Pets");
        }
    }
}
