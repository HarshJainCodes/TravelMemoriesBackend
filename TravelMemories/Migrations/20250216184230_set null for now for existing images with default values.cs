using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMemories.Migrations
{
    /// <inheritdoc />
    public partial class setnullfornowforexistingimageswithdefaultvalues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UploadedByEmail",
                table: "ImageMetadata",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "harshjain17may@gmail.com");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UserInfo_Email",
                table: "UserInfo",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_ImageMetadata_UploadedByEmail",
                table: "ImageMetadata",
                column: "UploadedByEmail");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageMetadata_UserInfo_UploadedByEmail",
                table: "ImageMetadata",
                column: "UploadedByEmail",
                principalTable: "UserInfo",
                principalColumn: "Email",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageMetadata_UserInfo_UploadedByEmail",
                table: "ImageMetadata");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_UserInfo_Email",
                table: "UserInfo");

            migrationBuilder.DropIndex(
                name: "IX_ImageMetadata_UploadedByEmail",
                table: "ImageMetadata");

            migrationBuilder.DropColumn(
                name: "UploadedByEmail",
                table: "ImageMetadata");
        }
    }
}
