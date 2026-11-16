using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pawhaven.Migrations
{
    /// <inheritdoc />
    public partial class AddGroomingServiceDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "GroomingServices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 60);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "GroomingServices");
        }
    }
}
