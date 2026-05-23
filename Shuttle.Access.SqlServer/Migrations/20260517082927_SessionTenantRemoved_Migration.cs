using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class SessionTenantRemoved_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_Tenant_TenantId",
                schema: "access",
                table: "Session");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SessionPermission",
                schema: "access",
                table: "SessionPermission");

            migrationBuilder.DropIndex(
                name: "IX_Session_TenantId",
                schema: "access",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "UX_Session_IdentityId_TenantId",
                schema: "access",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "UX_Session_Token",
                schema: "access",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Token",
                schema: "access",
                table: "Session");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "access",
                table: "SessionPermission",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                schema: "access",
                table: "Session",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SessionPermission",
                schema: "access",
                table: "SessionPermission",
                columns: new[] { "SessionId", "TenantId", "PermissionId" });

            migrationBuilder.CreateIndex(
                name: "UX_Session_IdentityId",
                schema: "access",
                table: "Session",
                column: "IdentityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Session_TokenHash",
                schema: "access",
                table: "Session",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SessionPermission",
                schema: "access",
                table: "SessionPermission");

            migrationBuilder.DropIndex(
                name: "UX_Session_IdentityId",
                schema: "access",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "UX_Session_TokenHash",
                schema: "access",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "SessionPermission");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                schema: "access",
                table: "Session");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "access",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<byte[]>(
                name: "Token",
                schema: "access",
                table: "Session",
                type: "varbinary(900)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SessionPermission",
                schema: "access",
                table: "SessionPermission",
                columns: new[] { "SessionId", "PermissionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Session_TenantId",
                schema: "access",
                table: "Session",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UX_Session_IdentityId_TenantId",
                schema: "access",
                table: "Session",
                columns: new[] { "IdentityId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Session_Token",
                schema: "access",
                table: "Session",
                column: "Token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Tenant_TenantId",
                schema: "access",
                table: "Session",
                column: "TenantId",
                principalSchema: "access",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
