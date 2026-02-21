using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class AddPOProductFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductEPA",
                table: "PurchaseOrderItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "PurchaseOrderItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "PurchaseOrderItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ProductPurchasePrice",
                table: "PurchaseOrderItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductUOM",
                table: "PurchaseOrderItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_Products_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Products_ProductId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_ProductId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "ProductEPA",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "ProductPurchasePrice",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "ProductUOM",
                table: "PurchaseOrderItems");
        }
    }
}
