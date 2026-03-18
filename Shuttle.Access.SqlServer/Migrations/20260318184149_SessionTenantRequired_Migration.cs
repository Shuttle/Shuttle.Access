using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations;

/// <inheritdoc />
public partial class SessionTenantRequired_Migration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM [access].[Session]");

        migrationBuilder.DropForeignKey(
            name: "FK_Session_Tenant_TenantId",
            schema: "access",
            table: "Session");

        migrationBuilder.DropIndex(
            name: "UX_Session_IdentityId",
            schema: "access",
            table: "Session");

        migrationBuilder.DropIndex(
            name: "UX_Session_IdentityName",
            schema: "access",
            table: "Session");

        migrationBuilder.DropColumn(
            name: "IdentityName",
            schema: "access",
            table: "Session");

        migrationBuilder.AlterColumn<Guid>(
            name: "TenantId",
            schema: "access",
            table: "Session",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "UX_Session_IdentityId_TenantId",
            schema: "access",
            table: "Session",
            columns: new[] { "IdentityId", "TenantId" },
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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Session_Tenant_TenantId",
            schema: "access",
            table: "Session");

        migrationBuilder.DropIndex(
            name: "UX_Session_IdentityId_TenantId",
            schema: "access",
            table: "Session");

        migrationBuilder.AlterColumn<Guid>(
            name: "TenantId",
            schema: "access",
            table: "Session",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AddColumn<string>(
            name: "IdentityName",
            schema: "access",
            table: "Session",
            type: "nvarchar(320)",
            maxLength: 320,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "UX_Session_IdentityId",
            schema: "access",
            table: "Session",
            column: "IdentityId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_Session_IdentityName",
            schema: "access",
            table: "Session",
            column: "IdentityName",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Session_Tenant_TenantId",
            schema: "access",
            table: "Session",
            column: "TenantId",
            principalSchema: "access",
            principalTable: "Tenant",
            principalColumn: "Id");
    }
}