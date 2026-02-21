using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class MakeQuoteIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_QuoteInventories_QuoteId_InventoryId",
                table: "Invoices");

            migrationBuilder.AlterColumn<int>(
                name: "QuoteId",
                table: "Invoices",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_QuoteInventories_QuoteId_InventoryId",
                table: "Invoices",
                columns: new[] { "QuoteId", "InventoryId" },
                principalTable: "QuoteInventories",
                principalColumns: new[] { "QuoteId", "InventoryId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_QuoteInventories_QuoteId_InventoryId",
                table: "Invoices");

            migrationBuilder.AlterColumn<int>(
                name: "QuoteId",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_QuoteInventories_QuoteId_InventoryId",
                table: "Invoices",
                columns: new[] { "QuoteId", "InventoryId" },
                principalTable: "QuoteInventories",
                principalColumns: new[] { "QuoteId", "InventoryId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
