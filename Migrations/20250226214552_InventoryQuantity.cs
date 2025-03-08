using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class InventoryQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InventoryId1",
                table: "PurchaseOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantityOnHand",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityQuoted",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightedAveragePrice",
                table: "Inventories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_InventoryId1",
                table: "PurchaseOrders",
                column: "InventoryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Inventories_InventoryId1",
                table: "PurchaseOrders",
                column: "InventoryId1",
                principalTable: "Inventories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Inventories_InventoryId1",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_InventoryId1",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "InventoryId1",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "QuantityOnHand",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "QuantityQuoted",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WeightedAveragePrice",
                table: "Inventories");
        }
    }
}
