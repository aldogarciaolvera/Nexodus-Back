using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexodus_Back.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAiGeneratedToRoutine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAiGenerated",
                table: "Routines",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAiGenerated",
                table: "Routines");
        }
    }
}
