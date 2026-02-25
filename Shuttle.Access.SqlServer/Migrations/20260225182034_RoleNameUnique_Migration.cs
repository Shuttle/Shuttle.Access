using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class RoleNameUnique_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Role_Name",
                schema: "access",
                table: "Role");

            migrationBuilder.CreateIndex(
                name: "UX_TenantId_Role_Name",
                schema: "access",
                table: "Role",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_TenantId_Role_Name",
                schema: "access",
                table: "Role");

            migrationBuilder.CreateIndex(
                name: "UX_Role_Name",
                schema: "access",
                table: "Role",
                column: "Name",
                unique: true);
        }
    }
}
