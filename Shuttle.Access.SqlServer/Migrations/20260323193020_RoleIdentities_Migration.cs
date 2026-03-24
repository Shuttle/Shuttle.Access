using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class RoleIdentities_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRole_Role_TenantId_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropIndex(
                name: "IX_IdentityRole_TenantId_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.RenameIndex(
                name: "UX_TenantId_Role_Name",
                schema: "access",
                table: "Role",
                newName: "UX_Role_TenantId_Name");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Role_Id",
                schema: "access",
                table: "Role",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "UX_Role_Id",
                schema: "access",
                table: "Role",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRole_RoleId",
                schema: "access",
                table: "IdentityRole",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRole_Role_RoleId",
                schema: "access",
                table: "IdentityRole",
                column: "RoleId",
                principalSchema: "access",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRole_Role_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Role_Id",
                schema: "access",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "UX_Role_Id",
                schema: "access",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_IdentityRole_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.RenameIndex(
                name: "UX_Role_TenantId_Name",
                schema: "access",
                table: "Role",
                newName: "UX_TenantId_Role_Name");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRole_TenantId_RoleId",
                schema: "access",
                table: "IdentityRole",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRole_Role_TenantId_RoleId",
                schema: "access",
                table: "IdentityRole",
                columns: new[] { "TenantId", "RoleId" },
                principalSchema: "access",
                principalTable: "Role",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
