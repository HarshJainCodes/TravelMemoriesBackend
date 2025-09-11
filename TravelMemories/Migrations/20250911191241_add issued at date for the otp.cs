using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMemories.Migrations
{
    /// <inheritdoc />
    public partial class addissuedatdatefortheotp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedAt",
                table: "VerificationCodes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuedAt",
                table: "VerificationCodes");
        }
    }
}
