using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class Schema_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "access");

            migrationBuilder.Sql("""
        IF OBJECT_ID('access.[Session]') IS NULL
            ALTER SCHEMA access TRANSFER dbo.[Session];
        """);

            migrationBuilder.Sql("""
        IF OBJECT_ID('access.[SessionTokenExchange]') IS NULL
            ALTER SCHEMA access TRANSFER dbo.[SessionTokenExchange];
        """);

            migrationBuilder.Sql("""
        IF OBJECT_ID('access.[SessionPermission]') IS NULL
            ALTER SCHEMA access TRANSFER dbo.[SessionPermission];
        """);

            migrationBuilder.Sql("""
        IF OBJECT_ID('access.[Role]') IS NULL
            ALTER SCHEMA access TRANSFER dbo.[Role];
        """);

            migrationBuilder.Sql("""
        IF OBJECT_ID('access.[RolePermission]') IS NULL
            ALTER SCHEMA access TRANSFER dbo.[RolePermission];
        """);

            migrationBuilder.Sql("""
        IF OBJECT_ID('access.[Identity]') IS NULL
            ALTER SCHEMA access TRANSFER dbo.[Identity];
        """);

            migrationBuilder.Sql("""
        IF OBJECT_ID('access.[IdentityRole]') IS NULL
            ALTER SCHEMA access TRANSFER dbo.[IdentityRole];
        """);

            migrationBuilder.Sql("""
        IF OBJECT_ID('access.[Permission]') IS NULL
            ALTER SCHEMA access TRANSFER dbo.[Permission];
        """);

            migrationBuilder.Sql("""
        IF OBJECT_ID('access.[__EFMigrationsHistory]') IS NULL
            ALTER SCHEMA access TRANSFER dbo.[__EFMigrationsHistory];
        """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID('dbo.[PrimitiveEvent]') IS NOT NULL AND OBJECT_ID('access.[PrimitiveEvent]') IS NULL
                    ALTER SCHEMA access TRANSFER dbo.[PrimitiveEvent];
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID('dbo.[EventType]') IS NOT NULL AND OBJECT_ID('access.[EventType]') IS NULL
                    ALTER SCHEMA access TRANSFER dbo.[EventType];
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID('dbo.[IdKey]') IS NOT NULL AND OBJECT_ID('access.[IdKey]') IS NULL
                    ALTER SCHEMA access TRANSFER dbo.[IdKey];
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID('dbo.[Projection]') IS NOT NULL AND OBJECT_ID('access.[Projection]') IS NULL
                    ALTER SCHEMA access TRANSFER dbo.[Projection];
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID('dbo.[ProjectionJournal]') IS NOT NULL AND OBJECT_ID('access.[ProjectionJournal]') IS NULL
                    ALTER SCHEMA access TRANSFER dbo.[ProjectionJournal];
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID('dbo.[SubscriberMessageType]') IS NOT NULL AND OBJECT_ID('access.[SubscriberMessageType]') IS NULL
                    ALTER SCHEMA access TRANSFER dbo.[SubscriberMessageType];
                """);

            migrationBuilder.Sql("DELETE FROM [access].[SessionPermission]");
            migrationBuilder.Sql("DELETE FROM [access].[Session]");

            migrationBuilder.RenameColumn(
                name: "IdentityId",
                schema: "access",
                table: "SessionPermission",
                newName: "SessionId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "access",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Session",
                schema: "access",
                table: "Session");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Session",
                schema: "access",
                table: "Session",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPermission_PermissionId",
                schema: "access",
                table: "SessionPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "UX_Session_IdentityId",
                schema: "access",
                table: "Session",
                column: "IdentityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                schema: "access",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityRole_RoleId",
                schema: "access",
                table: "IdentityRole",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRole_Identity_IdentityId",
                schema: "access",
                table: "IdentityRole",
                column: "IdentityId",
                principalSchema: "access",
                principalTable: "Identity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_RolePermission_Permission_PermissionId",
                schema: "access",
                table: "RolePermission",
                column: "PermissionId",
                principalSchema: "access",
                principalTable: "Permission",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Identity_IdentityId",
                schema: "access",
                table: "Session",
                column: "IdentityId",
                principalSchema: "access",
                principalTable: "Identity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionPermission_Permission_PermissionId",
                schema: "access",
                table: "SessionPermission",
                column: "PermissionId",
                principalSchema: "access",
                principalTable: "Permission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionPermission_Session_SessionId",
                schema: "access",
                table: "SessionPermission",
                column: "SessionId",
                principalSchema: "access",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRole_Identity_IdentityId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRole_Role_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Permission_PermissionId",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Role_RoleId",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Identity_IdentityId",
                schema: "access",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionPermission_Permission_PermissionId",
                schema: "access",
                table: "SessionPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionPermission_Session_SessionId",
                schema: "access",
                table: "SessionPermission");

            migrationBuilder.DropIndex(
                name: "IX_SessionPermission_PermissionId",
                schema: "access",
                table: "SessionPermission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Session",
                schema: "access",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "UX_Session_IdentityId",
                schema: "access",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_RolePermission_PermissionId",
                schema: "access",
                table: "RolePermission");

            migrationBuilder.DropIndex(
                name: "IX_IdentityRole_RoleId",
                schema: "access",
                table: "IdentityRole");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "access",
                table: "Session");

            migrationBuilder.RenameTable(
                name: "SessionTokenExchange",
                schema: "access",
                newName: "SessionTokenExchange");

            migrationBuilder.RenameTable(
                name: "SessionPermission",
                schema: "access",
                newName: "SessionPermission");

            migrationBuilder.RenameTable(
                name: "Session",
                schema: "access",
                newName: "Session");

            migrationBuilder.RenameTable(
                name: "RolePermission",
                schema: "access",
                newName: "RolePermission");

            migrationBuilder.RenameTable(
                name: "Role",
                schema: "access",
                newName: "Role");

            migrationBuilder.RenameTable(
                name: "Permission",
                schema: "access",
                newName: "Permission");

            migrationBuilder.RenameTable(
                name: "IdentityRole",
                schema: "access",
                newName: "IdentityRole");

            migrationBuilder.RenameTable(
                name: "Identity",
                schema: "access",
                newName: "Identity");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "SessionPermission",
                newName: "IdentityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Session",
                table: "Session",
                column: "IdentityId");
        }
    }
}
