using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIdToLoadMixDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "LoadMixDetails",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadMixDetails_ProductId",
                table: "LoadMixDetails",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoadMixDetails_Products_ProductId",
                table: "LoadMixDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadMixDetails_Products_ProductId",
                table: "LoadMixDetails");

            migrationBuilder.DropIndex(
                name: "IX_LoadMixDetails_ProductId",
                table: "LoadMixDetails");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "LoadMixDetails");
        }
    }
}
