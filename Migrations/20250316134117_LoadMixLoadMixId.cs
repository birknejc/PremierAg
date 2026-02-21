using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS.Migrations
{
    /// <inheritdoc />
    public partial class LoadMixLoadMixId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoadMixId",
                table: "LoadMixes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoadMixId",
                table: "LoadMixes");
        }
    }
}
