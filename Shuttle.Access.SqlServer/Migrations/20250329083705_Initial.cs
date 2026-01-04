using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    DateRegistered = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RegisteredBy = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    GeneratedPassword = table.Column<string>(type: "nvarchar(65)", maxLength: 65, nullable: true),
                    DateActivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityRole",
                columns: table => new
                {
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateRegistered = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityRole", x => new { x.IdentityId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateRegistered = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.PermissionId });
                });

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityName = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Token = table.Column<byte[]>(type: "varbinary(900)", nullable: false),
                    DateRegistered = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.IdentityId);
                });

            migrationBuilder.CreateTable(
                name: "SessionPermission",
                columns: table => new
                {
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionPermission", x => new { x.IdentityId, x.PermissionName });
                });

            migrationBuilder.CreateTable(
                name: "SessionTokenExchange",
                columns: table => new
                {
                    ExchangeToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTokenExchange", x => x.ExchangeToken);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Identity_Name",
                table: "Identity",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Permission_Name",
                table: "Permission",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Role_Name",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Session_IdentityName",
                table: "Session",
                column: "IdentityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Session_Token",
                table: "Session",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Identity");

            migrationBuilder.DropTable(
                name: "IdentityRole");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropTable(
                name: "SessionPermission");

            migrationBuilder.DropTable(
                name: "SessionTokenExchange");
        }
    }
}
