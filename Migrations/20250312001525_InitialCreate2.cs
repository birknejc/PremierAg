using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerBusinessName = table.Column<string>(type: "text", nullable: false),
                    CustomerFName = table.Column<string>(type: "text", nullable: false),
                    CustomerLName = table.Column<string>(type: "text", nullable: false),
                    CustomerStreet = table.Column<string>(type: "text", nullable: false),
                    CustomerCity = table.Column<string>(type: "text", nullable: false),
                    CustomerState = table.Column<string>(type: "text", nullable: false),
                    CustomerZipCode = table.Column<string>(type: "text", nullable: false),
                    CustomerPhone = table.Column<string>(type: "text", nullable: false),
                    CustomerCell = table.Column<string>(type: "text", nullable: false),
                    CustomerFax = table.Column<string>(type: "text", nullable: false),
                    CustomerEmail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UOMConversions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PUOM = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CFPurSold = table.Column<decimal>(type: "numeric", nullable: false),
                    SUOM = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QUOM = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConversionFactor = table.Column<decimal>(type: "numeric(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UOMConversions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessName = table.Column<string>(type: "text", nullable: false),
                    StreetAddress = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    ZipCode = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Fax = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    SalesRepName = table.Column<string>(type: "text", nullable: false),
                    SalesRepPhone = table.Column<string>(type: "text", nullable: false),
                    SalesRepEmail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FieldName = table.Column<string>(type: "text", nullable: false),
                    Acres = table.Column<double>(type: "double precision", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fields_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoQuoteInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceCustomer = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    InvoiceChemicalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    InvoiceUnitOfMeasure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoicePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    InvoiceRatePerAcre = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsPrinted = table.Column<bool>(type: "boolean", nullable: false),
                    IsGroupSelected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoQuoteInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoQuoteInvoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    CustomerBusinessName = table.Column<string>(type: "text", nullable: false),
                    QuoteStreet = table.Column<string>(type: "text", nullable: false),
                    QuoteCity = table.Column<string>(type: "text", nullable: false),
                    QuoteState = table.Column<string>(type: "text", nullable: false),
                    QuoteZipcode = table.Column<string>(type: "text", nullable: false),
                    QuotePhone = table.Column<string>(type: "text", nullable: false),
                    QuoteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuoteTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    EstimatedAcres = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotes_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChemicalName = table.Column<string>(type: "text", nullable: false),
                    EPA = table.Column<string>(type: "text", nullable: false),
                    UnitOfMeasurePurchase = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric", nullable: false),
                    WeightedAveragePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantityQuoted = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadMixes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuoteId = table.Column<int>(type: "integer", nullable: true),
                    LoadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LoadTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Crop = table.Column<string>(type: "text", nullable: false),
                    TotalGallons = table.Column<int>(type: "integer", nullable: false),
                    TotalAcres = table.Column<int>(type: "integer", nullable: false),
                    LMRatePerAcre = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadMixes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadMixes_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PONumber = table.Column<string>(type: "text", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BusinessName = table.Column<string>(type: "text", nullable: false),
                    PaymentDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveryPickUpDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PickUpLocation = table.Column<string>(type: "text", nullable: false),
                    InventoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuoteInventories",
                columns: table => new
                {
                    QuoteId = table.Column<int>(type: "integer", nullable: false),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ChemicalName = table.Column<string>(type: "text", nullable: false),
                    EPA = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantityPerAcre = table.Column<decimal>(type: "numeric", nullable: false),
                    QuotePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    QuoteUnitOfMeasure = table.Column<string>(type: "text", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValueSql: "gen_random_bytes(8)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteInventories", x => new { x.QuoteId, x.InventoryId });
                    table.ForeignKey(
                        name: "FK_QuoteInventories_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuoteInventories_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoadMixId = table.Column<int>(type: "integer", nullable: false),
                    SelectedFieldId = table.Column<int>(type: "integer", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: false),
                    FieldAverageRate = table.Column<decimal>(type: "numeric", nullable: false),
                    FieldTotalGallons = table.Column<decimal>(type: "numeric", nullable: false),
                    FieldAcres = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadFields_LoadMixes_LoadMixId",
                        column: x => x.LoadMixId,
                        principalTable: "LoadMixes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadMixDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoadMixId = table.Column<int>(type: "integer", nullable: false),
                    Product = table.Column<string>(type: "text", nullable: false),
                    RatePerAcre = table.Column<string>(type: "text", nullable: false),
                    Total = table.Column<string>(type: "text", nullable: false),
                    EPA = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    QuotePrice = table.Column<decimal>(type: "numeric", nullable: true),
                    QuoteUnitOfMeasure = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadMixDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadMixDetails_LoadMixes_LoadMixId",
                        column: x => x.LoadMixId,
                        principalTable: "LoadMixes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderId = table.Column<int>(type: "integer", nullable: false),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    ChemicalName = table.Column<string>(type: "text", nullable: false),
                    UnitOfMeasurePurchase = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    QuantityOrdered = table.Column<int>(type: "integer", nullable: false),
                    QuantityReceived = table.Column<int>(type: "integer", nullable: false),
                    NewQuantityReceived = table.Column<int>(type: "integer", nullable: false),
                    QuantityUnreceived = table.Column<int>(type: "integer", nullable: false),
                    EPANumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceCustomer = table.Column<string>(type: "text", nullable: false),
                    InvoiceRatePerAcre = table.Column<decimal>(type: "numeric", nullable: false),
                    InvoiceUnitOfMeasure = table.Column<string>(type: "text", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: false),
                    InvoiceChemicalName = table.Column<string>(type: "text", nullable: false),
                    InvoicePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsPrinted = table.Column<bool>(type: "boolean", nullable: false),
                    IsGroupSelected = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "IX_Fields_CustomerId",
                table: "Fields",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_VendorId",
                table: "Inventories",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_QuoteId_InventoryId",
                table: "Invoices",
                columns: new[] { "QuoteId", "InventoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_LoadFields_LoadMixId",
                table: "LoadFields",
                column: "LoadMixId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadMixDetails_LoadMixId",
                table: "LoadMixDetails",
                column: "LoadMixId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadMixes_QuoteId",
                table: "LoadMixes",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_NoQuoteInvoices_CustomerId",
                table: "NoQuoteInvoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_InventoryId",
                table: "PurchaseOrderItems",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_InventoryId",
                table: "PurchaseOrders",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteInventories_InventoryId",
                table: "QuoteInventories",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CustomerId",
                table: "Quotes",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "LoadFields");

            migrationBuilder.DropTable(
                name: "LoadMixDetails");

            migrationBuilder.DropTable(
                name: "NoQuoteInvoices");

            migrationBuilder.DropTable(
                name: "PurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "UOMConversions");

            migrationBuilder.DropTable(
                name: "QuoteInventories");

            migrationBuilder.DropTable(
                name: "LoadMixes");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Vendors");
        }
    }
}
