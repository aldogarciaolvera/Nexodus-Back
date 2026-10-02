using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexodus_Back.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotificationTime",
                table: "TodoList",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationTime",
                table: "TodoList");
        }
    }
}
