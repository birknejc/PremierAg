using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop remaining FK constraints that still exist in the DB
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Inventories_InventoryId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteInventories_Inventories_InventoryId",
                table: "QuoteInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Inventories_InventoryId",
                table: "PurchaseOrderItems");

            // Drop the actual Inventory table
            migrationBuilder.DropTable(name: "Inventories");

            // Drop LoadMix2 tables
            migrationBuilder.DropTable(name: "LoadMix2Details");
            migrationBuilder.DropTable(name: "LoadMix2Fields");
            migrationBuilder.DropTable(name: "LoadMix2s");

            // Drop InventoryId columns
            migrationBuilder.DropColumn(name: "InventoryId", table: "QuoteInventories");
            migrationBuilder.DropColumn(name: "InventoryId", table: "PurchaseOrders");
            migrationBuilder.DropColumn(name: "InventoryId", table: "PurchaseOrderItems");
        }





        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback — legacy tables removed permanently.
        }

    }
}
