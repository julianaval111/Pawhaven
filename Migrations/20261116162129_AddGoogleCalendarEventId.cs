using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pawhaven.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleCalendarEventId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleCalendarEventId",
                table: "Bookings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleCalendarEventId",
                table: "Bookings");
        }
    }
}
