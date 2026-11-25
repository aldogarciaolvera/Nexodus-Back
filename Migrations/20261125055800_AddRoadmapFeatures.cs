using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexodus_Back.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadmapFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CardioMinutes",
                table: "Routines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardioType",
                table: "Routines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeCardio",
                table: "Routines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                table: "Routines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "Routines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoutineType",
                table: "Routines",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Strength");

            migrationBuilder.AddColumn<string>(
                name: "WarmupExercisesJson",
                table: "Routines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarmupMinutes",
                table: "Routines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FoodBudgetAmount",
                table: "DietTargets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FoodBudgetCategoryId",
                table: "DietTargets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoodBudgetCurrency",
                table: "DietTargets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FoodBudgetPeriod",
                table: "DietTargets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CardioSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Environment = table.Column<string>(type: "text", nullable: false),
                    ActivityType = table.Column<string>(type: "text", nullable: false),
                    TrackingMode = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TotalDurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    DistanceKm = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedCalories = table.Column<double>(type: "double precision", nullable: false),
                    IsAiGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardioSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardioSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CardioSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentOrder = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardioSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardioSegments_CardioSessions_CardioSessionId",
                        column: x => x.CardioSessionId,
                        principalTable: "CardioSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardioSegments_CardioSessionId",
                table: "CardioSegments",
                column: "CardioSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CardioSessions_UserId_StartTime",
                table: "CardioSessions",
                columns: new[] { "UserId", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardioSegments");

            migrationBuilder.DropTable(
                name: "CardioSessions");

            migrationBuilder.DropColumn(
                name: "CardioMinutes",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "CardioType",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "IncludeCardio",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "RoutineType",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "WarmupExercisesJson",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "WarmupMinutes",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "FoodBudgetAmount",
                table: "DietTargets");

            migrationBuilder.DropColumn(
                name: "FoodBudgetCategoryId",
                table: "DietTargets");

            migrationBuilder.DropColumn(
                name: "FoodBudgetCurrency",
                table: "DietTargets");

            migrationBuilder.DropColumn(
                name: "FoodBudgetPeriod",
                table: "DietTargets");
        }
    }
}
