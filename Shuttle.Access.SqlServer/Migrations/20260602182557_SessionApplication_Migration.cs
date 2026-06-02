using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class SessionApplication_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Session_IdentityId",
                schema: "access",
                table: "Session");

            migrationBuilder.AddColumn<string>(
                name: "Application",
                schema: "access",
                table: "Session",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "Access");

            migrationBuilder.CreateIndex(
                name: "UX_Session_IdentityId_Application",
                schema: "access",
                table: "Session",
                columns: new[] { "IdentityId", "Application" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Session_IdentityId_Application",
                schema: "access",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Application",
                schema: "access",
                table: "Session");

            migrationBuilder.CreateIndex(
                name: "UX_Session_IdentityId",
                schema: "access",
                table: "Session",
                column: "IdentityId",
                unique: true);
        }
    }
}
