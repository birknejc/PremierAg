using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class FinalFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FakeColumn2",
                table: "Invoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FakeColumn2",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
