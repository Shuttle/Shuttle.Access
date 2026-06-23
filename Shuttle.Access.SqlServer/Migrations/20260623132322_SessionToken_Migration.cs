using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class SessionToken_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [access].[Session]");

            migrationBuilder.DropIndex(
                name: "UX_Session_IdentityId_Application",
                schema: "access",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "UX_Session_TokenHash",
                schema: "access",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Application",
                schema: "access",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                schema: "access",
                table: "Session");

            migrationBuilder.CreateTable(
                name: "SessionTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DateRegistered = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Application = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionTokens_Session_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "access",
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Session_IdentityId",
                schema: "access",
                table: "Session",
                column: "IdentityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_SessionToken_SessionId_TokenHash",
                table: "SessionTokens",
                columns: new[] { "SessionId", "TokenHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionTokens");

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

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                schema: "access",
                table: "Session",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UX_Session_IdentityId_Application",
                schema: "access",
                table: "Session",
                columns: new[] { "IdentityId", "Application" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Session_TokenHash",
                schema: "access",
                table: "Session",
                column: "TokenHash",
                unique: true);
        }
    }
}
