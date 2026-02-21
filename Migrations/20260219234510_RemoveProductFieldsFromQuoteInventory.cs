using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductFieldsFromQuoteInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductEPA",
                table: "QuoteInventories");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "QuoteInventories");

            migrationBuilder.DropColumn(
                name: "ProductUOM",
                table: "QuoteInventories");

            migrationBuilder.DropColumn(
                name: "ProductWeightedAveragePrice",
                table: "QuoteInventories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductEPA",
                table: "QuoteInventories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "QuoteInventories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductUOM",
                table: "QuoteInventories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ProductWeightedAveragePrice",
                table: "QuoteInventories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
