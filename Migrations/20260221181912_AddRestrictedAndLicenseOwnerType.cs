using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class AddRestrictedAndLicenseOwnerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Restricted",
                table: "Products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerType",
                table: "ApplicatorLicenses",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Restricted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "ApplicatorLicenses");
        }
    }
}
