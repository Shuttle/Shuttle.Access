using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuttle.Access.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class IdentityAlternateName_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdentityAlternateName",
                schema: "access",
                columns: table => new
                {
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityAlternateName", x => new { x.IdentityId, x.Name });
                    table.ForeignKey(
                        name: "FK_IdentityAlternateName_Identity_IdentityId",
                        column: x => x.IdentityId,
                        principalSchema: "access",
                        principalTable: "Identity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_IdentityAlternateName_Name",
                schema: "access",
                table: "IdentityAlternateName",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityAlternateName",
                schema: "access");
        }
    }
}
