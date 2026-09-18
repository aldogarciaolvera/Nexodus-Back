using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexodus_Back.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitTrackingToTodoList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentStreak",
                table: "TodoList",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomDays",
                table: "TodoList",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "TodoList",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "TodoList",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsHabit",
                table: "TodoList",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCompletedAt",
                table: "TodoList",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStreak",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "CustomDays",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "IsHabit",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "LastCompletedAt",
                table: "TodoList");
        }
    }
}
