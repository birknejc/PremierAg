using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class Invoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuoteInventory_Inventories_InventoryId",
                table: "QuoteInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteInventory_Quotes_QuoteId",
                table: "QuoteInventory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuoteInventory",
                table: "QuoteInventory");

            migrationBuilder.RenameTable(
                name: "QuoteInventory",
                newName: "QuoteInventories");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteInventory_InventoryId",
                table: "QuoteInventories",
                newName: "IX_QuoteInventories_InventoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuoteInventories",
                table: "QuoteInventories",
                columns: new[] { "QuoteId", "InventoryId" });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceCustomer = table.Column<string>(type: "text", nullable: false),
                    InvoiceRatePerAcre = table.Column<decimal>(type: "numeric", nullable: false),
                    InvoiceUnitOfMeasure = table.Column<string>(type: "text", nullable: false),
                    InvoiceChemicalName = table.Column<string>(type: "text", nullable: false),
                    InvoicePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    QuoteId = table.Column<int>(type: "integer", nullable: false),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_QuoteInventories_QuoteId_InventoryId",
                        columns: x => new { x.QuoteId, x.InventoryId },
                        principalTable: "QuoteInventories",
                        principalColumns: new[] { "QuoteId", "InventoryId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_QuoteId_InventoryId",
                table: "Invoices",
                columns: new[] { "QuoteId", "InventoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteInventories_Inventories_InventoryId",
                table: "QuoteInventories",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteInventories_Quotes_QuoteId",
                table: "QuoteInventories",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuoteInventories_Inventories_InventoryId",
                table: "QuoteInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteInventories_Quotes_QuoteId",
                table: "QuoteInventories");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuoteInventories",
                table: "QuoteInventories");

            migrationBuilder.RenameTable(
                name: "QuoteInventories",
                newName: "QuoteInventory");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteInventories_InventoryId",
                table: "QuoteInventory",
                newName: "IX_QuoteInventory_InventoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuoteInventory",
                table: "QuoteInventory",
                columns: new[] { "QuoteId", "InventoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteInventory_Inventories_InventoryId",
                table: "QuoteInventory",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteInventory_Quotes_QuoteId",
                table: "QuoteInventory",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
