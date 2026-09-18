using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexodus_Back.Migrations
{
    /// <inheritdoc />
    public partial class AddSubtitleTagUrgentToTodoList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "TodoList",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                table: "TodoList",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Urgent",
                table: "TodoList",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "Urgent",
                table: "TodoList");
        }
    }
}
