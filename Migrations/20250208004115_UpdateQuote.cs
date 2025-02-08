using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "QuoteInventory");

            migrationBuilder.DropColumn(
                name: "QuoteCost",
                table: "QuoteInventory");

            migrationBuilder.RenameColumn(
                name: "CostUnitOfMeasure",
                table: "QuoteInventory",
                newName: "ChemicalName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChemicalName",
                table: "QuoteInventory",
                newName: "CostUnitOfMeasure");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "QuoteInventory",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuoteCost",
                table: "QuoteInventory",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
