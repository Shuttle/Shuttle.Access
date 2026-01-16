using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class Tenant_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
delete from [access].[Projection]
delete from [access].[SessionPermission]
delete from [access].[Session]
delete from [access].[RolePermission]
delete from [access].[Role]
delete from [access].[IdentityRole]
delete from [access].[Identity]
delete from [access].[Permission]
");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRole_Role_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Role_RoleId",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolePermission",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Role",
                schema: "access",
                table: "Role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityRole",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropIndex(
                name: "IX_IdentityRole_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropColumn(
                name: "DateRegistered",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "DateRegistered",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "access",
                table: "SessionPermission",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "access",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "access",
                table: "RolePermission",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleTenantId",
                schema: "access",
                table: "RolePermission",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "access",
                table: "Role",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                schema: "access",
                table: "Permission",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "access",
                table: "IdentityRole",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleTenantId",
                schema: "access",
                table: "IdentityRole",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolePermission",
                schema: "access",
                table: "RolePermission",
                columns: new[] { "TenantId", "RoleId", "PermissionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Role",
                schema: "access",
                table: "Role",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityRole",
                schema: "access",
                table: "IdentityRole",
                columns: new[] { "TenantId", "IdentityId", "RoleId" });

            migrationBuilder.CreateTable(
                name: "Tenant",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityTenant",
                schema: "access",
                columns: table => new
                {
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityTenant", x => new { x.IdentityId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_IdentityTenant_Identity_IdentityId",
                        column: x => x.IdentityId,
                        principalSchema: "access",
                        principalTable: "Identity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IdentityTenant_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "access",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionTenant",
                schema: "access",
                columns: table => new
                {
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionTenant", x => new { x.PermissionId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_PermissionTenant_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "access",
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionTenant_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "access",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_RoleTenantId_RoleId",
                schema: "access",
                table: "RolePermission",
                columns: new[] { "RoleTenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRole_IdentityId",
                schema: "access",
                table: "IdentityRole",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRole_RoleTenantId_RoleId",
                schema: "access",
                table: "IdentityRole",
                columns: new[] { "RoleTenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityTenant_TenantId",
                schema: "access",
                table: "IdentityTenant",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTenant_TenantId",
                schema: "access",
                table: "PermissionTenant",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UX_Tenant_Name",
                schema: "access",
                table: "Tenant",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRole_Role_RoleTenantId_RoleId",
                schema: "access",
                table: "IdentityRole",
                columns: new[] { "RoleTenantId", "RoleId" },
                principalSchema: "access",
                principalTable: "Role",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Role_RoleTenantId_RoleId",
                schema: "access",
                table: "RolePermission",
                columns: new[] { "RoleTenantId", "RoleId" },
                principalSchema: "access",
                principalTable: "Role",
                principalColumns: new[] { "TenantId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRole_Role_RoleTenantId_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Role_RoleTenantId_RoleId",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropTable(
                name: "IdentityTenant",
                schema: "access");

            migrationBuilder.DropTable(
                name: "PermissionTenant",
                schema: "access");

            migrationBuilder.DropTable(
                name: "Tenant",
                schema: "access");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolePermission",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropIndex(
                name: "IX_RolePermission_RoleTenantId_RoleId",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Role",
                schema: "access",
                table: "Role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityRole",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropIndex(
                name: "IX_IdentityRole_IdentityId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropIndex(
                name: "IX_IdentityRole_RoleTenantId_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "SessionPermission");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "RoleTenantId",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "Scope",
                schema: "access",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropColumn(
                name: "RoleTenantId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateRegistered",
                schema: "access",
                table: "RolePermission",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateRegistered",
                schema: "access",
                table: "IdentityRole",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolePermission",
                schema: "access",
                table: "RolePermission",
                columns: new[] { "RoleId", "PermissionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Role",
                schema: "access",
                table: "Role",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityRole",
                schema: "access",
                table: "IdentityRole",
                columns: new[] { "IdentityId", "RoleId" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Role_RoleId",
                schema: "access",
                table: "RolePermission",
                column: "RoleId",
                principalSchema: "access",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
