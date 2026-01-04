using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    /// <inheritdoc />
    public partial class PermissionDescription_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Permission",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Permission");
        }
    }
}
