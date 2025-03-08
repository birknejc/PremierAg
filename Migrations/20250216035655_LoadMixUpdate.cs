using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class LoadMixUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Product",
                table: "LoadMixes");

            migrationBuilder.DropColumn(
                name: "RatePerAcre",
                table: "LoadMixes");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "LoadMixes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Product",
                table: "LoadMixes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RatePerAcre",
                table: "LoadMixes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Total",
                table: "LoadMixes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
