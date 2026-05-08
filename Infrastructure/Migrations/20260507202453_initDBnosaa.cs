using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamkeen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initDBnosaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Tickets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Tickets");
        }
    }
}
