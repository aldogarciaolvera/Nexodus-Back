using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexodus_Back.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitsAndCompletions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Habits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    HabitType = table.Column<string>(type: "text", nullable: false),
                    TrackingType = table.Column<string>(type: "text", nullable: false),
                    TargetValue = table.Column<double>(type: "double precision", nullable: false),
                    TargetUnit = table.Column<string>(type: "text", nullable: true),
                    OnTapAction = table.Column<string>(type: "text", nullable: false),
                    FrequencyType = table.Column<string>(type: "text", nullable: false),
                    FrequencyDays = table.Column<string>(type: "text", nullable: true),
                    FrequencyTargetCount = table.Column<int>(type: "integer", nullable: false),
                    EveryNDaysInterval = table.Column<int>(type: "integer", nullable: false),
                    StreakUnit = table.Column<string>(type: "text", nullable: false),
                    StreakGoal = table.Column<int>(type: "integer", nullable: true),
                    FillMetPeriods = table.Column<bool>(type: "boolean", nullable: false),
                    AutoCompleteSource = table.Column<string>(type: "text", nullable: true),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false),
                    BestStreak = table.Column<int>(type: "integer", nullable: false),
                    LastCompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Habits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HabitCompletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HabitId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HabitCompletions_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HabitReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HabitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Time = table.Column<string>(type: "text", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HabitReminders_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_HabitId_Date",
                table: "HabitCompletions",
                columns: new[] { "HabitId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_UserId_Date",
                table: "HabitCompletions",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_HabitReminders_HabitId",
                table: "HabitReminders",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_Habits_AutoCompleteSource",
                table: "Habits",
                column: "AutoCompleteSource");

            migrationBuilder.CreateIndex(
                name: "IX_Habits_UserId_IsArchived",
                table: "Habits",
                columns: new[] { "UserId", "IsArchived" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HabitCompletions");

            migrationBuilder.DropTable(
                name: "HabitReminders");

            migrationBuilder.DropTable(
                name: "Habits");
        }
    }
}
