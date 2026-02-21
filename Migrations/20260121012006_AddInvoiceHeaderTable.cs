using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceHeaderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceCustomer",
                table: "Invoices");

            migrationBuilder.CreateTable(
                name: "InvoiceHeaders",
                columns: table => new
                {
                    InvoiceGroupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceHeaders", x => x.InvoiceGroupId);
                    table.ForeignKey(
                        name: "FK_InvoiceHeaders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceGroupId",
                table: "Invoices",
                column: "InvoiceGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeaders_CustomerId",
                table: "InvoiceHeaders",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_InvoiceHeaders_InvoiceGroupId",
                table: "Invoices",
                column: "InvoiceGroupId",
                principalTable: "InvoiceHeaders",
                principalColumn: "InvoiceGroupId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_InvoiceHeaders_InvoiceGroupId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "InvoiceHeaders");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceGroupId",
                table: "Invoices");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceCustomer",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
