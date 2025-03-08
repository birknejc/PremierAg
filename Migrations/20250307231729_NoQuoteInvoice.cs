using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class NoQuoteInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    IsPrinted = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_NoQuoteInvoices_CustomerId",
                table: "NoQuoteInvoices",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoQuoteInvoices");
        }
    }
}
