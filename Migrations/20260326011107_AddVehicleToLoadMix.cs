using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleToLoadMix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VehicleDescriptionSnapshot",
                table: "LoadMixes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "LoadMixes",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleDescriptionSnapshot",
                table: "LoadMixes");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "LoadMixes");
        }
    }
}
