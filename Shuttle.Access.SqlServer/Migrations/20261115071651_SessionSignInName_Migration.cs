using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class SessionSignInName_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignInName",
                schema: "access",
                table: "Session",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignInName",
                schema: "access",
                table: "Session");
        }
    }
}
