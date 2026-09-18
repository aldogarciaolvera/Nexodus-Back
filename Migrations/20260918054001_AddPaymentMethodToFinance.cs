using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexodus_Back.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodToFinance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Finances",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Finances");
        }
    }
}
