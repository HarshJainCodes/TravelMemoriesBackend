using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMemories.Migrations
{
    /// <inheritdoc />
    public partial class addoauthflow1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OAuthCodeStores",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginChallenge = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthCodeStores", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OAuthCodeStores_Code",
                table: "OAuthCodeStores",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OAuthCodeStores");
        }
    }
}
