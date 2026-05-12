using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10296771_CLDV7311_POE.Migrations
{
    /// <inheritdoc />
    public partial class SyncWithAzureDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Venue",
                newName: "ImageFileName");

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "Venue",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "Event",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Event",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Event");

            migrationBuilder.RenameColumn(
                name: "ImageFileName",
                table: "Venue",
                newName: "ImageUrl");
        }
    }
}
