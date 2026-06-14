using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Typer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchApiFootballFixtureId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApiFootballFixtureId",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ApiFootballFixtureId",
                table: "Matches",
                column: "ApiFootballFixtureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Matches_ApiFootballFixtureId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ApiFootballFixtureId",
                table: "Matches");
        }
    }
}
