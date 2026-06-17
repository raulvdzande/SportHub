using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.API.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGpsColumnsToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "GpsRadiusMeters",
                table: "Locations",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Locations",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Locations",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GpsRadiusMeters",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Locations");
        }
    }
}
