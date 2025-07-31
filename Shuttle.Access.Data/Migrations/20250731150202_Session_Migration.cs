using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class Session_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[SessionPermission]");
            migrationBuilder.Sql("DELETE FROM [dbo].[Session]");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SessionPermission",
                table: "SessionPermission");

            migrationBuilder.DropColumn(
                name: "PermissionName",
                table: "SessionPermission");

            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId",
                table: "SessionPermission",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SessionPermission",
                table: "SessionPermission",
                columns: new[] { "IdentityId", "PermissionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[SessionPermission]");
            migrationBuilder.Sql("DELETE FROM [dbo].[Session]");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SessionPermission",
                table: "SessionPermission");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "SessionPermission");

            migrationBuilder.AddColumn<string>(
                name: "PermissionName",
                table: "SessionPermission",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SessionPermission",
                table: "SessionPermission",
                columns: new[] { "IdentityId", "PermissionName" });
        }
    }
}
