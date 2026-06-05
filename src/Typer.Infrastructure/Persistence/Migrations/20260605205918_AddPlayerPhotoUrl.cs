using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Typer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerPhotoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Players",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Players");
        }
    }
}
