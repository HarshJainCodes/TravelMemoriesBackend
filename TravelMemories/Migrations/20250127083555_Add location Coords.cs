using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMemories.Migrations
{
    /// <inheritdoc />
    public partial class AddlocationCoords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "X",
                table: "ImageMetadata",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Y",
                table: "ImageMetadata",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "X",
                table: "ImageMetadata");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "ImageMetadata");
        }
    }
}
