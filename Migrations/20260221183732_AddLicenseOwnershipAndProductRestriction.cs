using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseOwnershipAndProductRestriction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "ApplicatorLicenses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicatorLicenses_CustomerId",
                table: "ApplicatorLicenses",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicatorLicenses_Customers_CustomerId",
                table: "ApplicatorLicenses",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicatorLicenses_Customers_CustomerId",
                table: "ApplicatorLicenses");

            migrationBuilder.DropIndex(
                name: "IX_ApplicatorLicenses_CustomerId",
                table: "ApplicatorLicenses");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "ApplicatorLicenses");
        }
    }
}
