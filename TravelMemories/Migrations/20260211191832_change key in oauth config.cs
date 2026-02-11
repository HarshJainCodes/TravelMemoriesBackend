using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMemories.Migrations
{
    /// <inheritdoc />
    public partial class changekeyinoauthconfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OAuthCodeStores",
                table: "OAuthCodeStores");

            migrationBuilder.DropIndex(
                name: "IX_OAuthCodeStores_Code",
                table: "OAuthCodeStores");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "OAuthCodeStores",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "OAuthCodeStores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OAuthCodeStores",
                table: "OAuthCodeStores",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_OAuthCodeStores_Email",
                table: "OAuthCodeStores",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OAuthCodeStores",
                table: "OAuthCodeStores");

            migrationBuilder.DropIndex(
                name: "IX_OAuthCodeStores_Email",
                table: "OAuthCodeStores");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "OAuthCodeStores",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "OAuthCodeStores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OAuthCodeStores",
                table: "OAuthCodeStores",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_OAuthCodeStores_Code",
                table: "OAuthCodeStores",
                column: "Code",
                unique: true);
        }
    }
}
