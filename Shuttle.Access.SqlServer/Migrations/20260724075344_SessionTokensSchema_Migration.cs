using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class SessionTokensSchema_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionTokens_Session_SessionId",
                table: "SessionTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SessionTokens",
                table: "SessionTokens");

            migrationBuilder.RenameTable(
                name: "SessionTokens",
                newName: "SessionToken",
                newSchema: "access");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SessionToken",
                schema: "access",
                table: "SessionToken",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionToken_Session_SessionId",
                schema: "access",
                table: "SessionToken",
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
                name: "FK_SessionToken_Session_SessionId",
                schema: "access",
                table: "SessionToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SessionToken",
                schema: "access",
                table: "SessionToken");

            migrationBuilder.RenameTable(
                name: "SessionToken",
                schema: "access",
                newName: "SessionTokens");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SessionTokens",
                table: "SessionTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionTokens_Session_SessionId",
                table: "SessionTokens",
                column: "SessionId",
                principalSchema: "access",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
