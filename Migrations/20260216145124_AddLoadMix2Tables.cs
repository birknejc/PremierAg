using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class AddLoadMix2Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoadMix2s",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoadMix2GroupId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_LoadMix2s", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadMix2s_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LoadMix2Details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoadMix2Id = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    RatePerAcre = table.Column<string>(type: "text", nullable: false),
                    Total = table.Column<string>(type: "text", nullable: false),
                    EPA = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    QuotePrice = table.Column<decimal>(type: "numeric", nullable: true),
                    QuoteUnitOfMeasure = table.Column<string>(type: "text", nullable: true),
                    ProductUnitOfMeasure = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadMix2Details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadMix2Details_LoadMix2s_LoadMix2Id",
                        column: x => x.LoadMix2Id,
                        principalTable: "LoadMix2s",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoadMix2Details_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoadMix2Fields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoadMix2Id = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    SelectedFieldId = table.Column<int>(type: "integer", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: false),
                    FieldAverageRate = table.Column<decimal>(type: "numeric", nullable: false),
                    FieldTotalGallons = table.Column<decimal>(type: "numeric", nullable: false),
                    FieldAcres = table.Column<decimal>(type: "numeric", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadMix2Fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadMix2Fields_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadMix2Fields_LoadMix2s_LoadMix2Id",
                        column: x => x.LoadMix2Id,
                        principalTable: "LoadMix2s",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadMix2Details_LoadMix2Id",
                table: "LoadMix2Details",
                column: "LoadMix2Id");

            migrationBuilder.CreateIndex(
                name: "IX_LoadMix2Details_ProductId",
                table: "LoadMix2Details",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadMix2Fields_CustomerId",
                table: "LoadMix2Fields",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadMix2Fields_LoadMix2Id",
                table: "LoadMix2Fields",
                column: "LoadMix2Id");

            migrationBuilder.CreateIndex(
                name: "IX_LoadMix2s_QuoteId",
                table: "LoadMix2s",
                column: "QuoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoadMix2Details");

            migrationBuilder.DropTable(
                name: "LoadMix2Fields");

            migrationBuilder.DropTable(
                name: "LoadMix2s");
        }
    }
}
