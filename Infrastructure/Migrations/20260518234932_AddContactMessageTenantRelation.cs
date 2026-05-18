using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tamkeen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactMessageTenantRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantName",
                table: "ContactMessages");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "ContactMessages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_TenantId",
                table: "ContactMessages",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactMessages_AspNetUsers_TenantId",
                table: "ContactMessages",
                column: "TenantId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactMessages_AspNetUsers_TenantId",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_TenantId",
                table: "ContactMessages");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "TenantName",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
