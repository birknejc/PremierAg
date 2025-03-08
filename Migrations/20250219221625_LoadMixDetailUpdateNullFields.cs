using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class LoadMixDetailUpdateNullFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadMixes_Quotes_QuoteId",
                table: "LoadMixes");

            migrationBuilder.AddColumn<string>(
                name: "EPA",
                table: "LoadMixDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "LoadMixDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuotePrice",
                table: "LoadMixDetails",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuoteUnitOfMeasure",
                table: "LoadMixDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LoadMixes_Quotes_QuoteId",
                table: "LoadMixes",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadMixes_Quotes_QuoteId",
                table: "LoadMixes");

            migrationBuilder.DropColumn(
                name: "EPA",
                table: "LoadMixDetails");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "LoadMixDetails");

            migrationBuilder.DropColumn(
                name: "QuotePrice",
                table: "LoadMixDetails");

            migrationBuilder.DropColumn(
                name: "QuoteUnitOfMeasure",
                table: "LoadMixDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_LoadMixes_Quotes_QuoteId",
                table: "LoadMixes",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id");
        }
    }
}
