using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMemories.Migrations
{
    /// <inheritdoc />
    public partial class Addsubscriptiontable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubscriptionType = table.Column<int>(type: "int", nullable: false),
                    PlanStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlanType = table.Column<int>(type: "int", nullable: false),
                    StorageUsedInGB = table.Column<float>(type: "real", nullable: false),
                    StorageCapacityInGB = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionDetails_UserInfo_UserEmail",
                        column: x => x.UserEmail,
                        principalTable: "UserInfo",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionDetails_UserEmail",
                table: "SubscriptionDetails",
                column: "UserEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionDetails");
        }
    }
}
