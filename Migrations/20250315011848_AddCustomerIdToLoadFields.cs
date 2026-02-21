using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdToLoadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "LoadFields",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LoadFields_CustomerId",
                table: "LoadFields",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoadFields_Customers_CustomerId",
                table: "LoadFields",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadFields_Customers_CustomerId",
                table: "LoadFields");

            migrationBuilder.DropIndex(
                name: "IX_LoadFields_CustomerId",
                table: "LoadFields");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "LoadFields");
        }
    }
}
