using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelMemories.Migrations
{
    /// <inheritdoc />
    public partial class UseOpenAIMessageFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageRole",
                table: "ChatMessages",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "ChatMessages",
                newName: "ReasoningContent");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ChatMessages",
                newName: "Timestamp");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ChatMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ToolCallId",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToolCalls",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ToolCallId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ToolCalls",
                table: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ChatMessages",
                newName: "MessageRole");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "ChatMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ReasoningContent",
                table: "ChatMessages",
                newName: "Message");
        }
    }
}
