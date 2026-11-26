using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexodus_Back.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGamification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGamifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false),
                    HighestStreak = table.Column<int>(type: "integer", nullable: false),
                    LastActiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StreakGracePeriodExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledRestDays = table.Column<string>(type: "text", nullable: false),
                    StreakFreezesAvailable = table.Column<int>(type: "integer", nullable: false),
                    NxpTotal = table.Column<int>(type: "integer", nullable: false),
                    Tier = table.Column<string>(type: "text", nullable: false),
                    Cyphers = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGamifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGamifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGamifications_UserId",
                table: "UserGamifications",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGamifications");
        }
    }
}
