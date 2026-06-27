using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualPet.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedingWindowLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FeedingWindowStartUtc",
                table: "Pets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeedingsUsedInWindow",
                table: "Pets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Pets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FeedingWindowStartUtc", "FeedingsUsedInWindow" },
                values: new object[] { null, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedingWindowStartUtc",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "FeedingsUsedInWindow",
                table: "Pets");
        }
    }
}
