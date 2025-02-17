using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMemories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDefaultvaluefromuploadedbyemailcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UploadedByEmail",
                table: "ImageMetadata",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldDefaultValue: "harshjain17may@gmail.com");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UploadedByEmail",
                table: "ImageMetadata",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "harshjain17may@gmail.com",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
