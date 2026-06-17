using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10296771_CLDV7311_POE.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeAndVenueAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Amenities",
                table: "Venue",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableFrom",
                table: "Venue",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableTo",
                table: "Venue",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DaysAvailable",
                table: "Venue",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasParking",
                table: "Venue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Venue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndoor",
                table: "Venue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWheelchairAccessible",
                table: "Venue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OperatingHours",
                table: "Venue",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Event",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxCapacity",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TicketPrice",
                table: "Event",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultCapacity = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Event_EventTypeId",
                table: "Event",
                column: "EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Event_EventTypes_EventTypeId",
                table: "Event",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_EventTypes_EventTypeId",
                table: "Event");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_Event_EventTypeId",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "AvailableTo",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "DaysAvailable",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "HasParking",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "IsIndoor",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "IsWheelchairAccessible",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "OperatingHours",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "TicketPrice",
                table: "Event");
        }
    }
}
