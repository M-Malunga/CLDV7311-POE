using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10296771_CLDV7311_POE.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Booking_VenueId",
                table: "Booking");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Venue_Date",
                table: "Booking",
                columns: new[] { "VenueId", "BookingDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Booking_Venue_Date",
                table: "Booking");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_VenueId",
                table: "Booking",
                column: "VenueId");
        }
    }
}
