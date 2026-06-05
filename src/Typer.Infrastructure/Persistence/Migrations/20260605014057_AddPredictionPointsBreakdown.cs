using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Typer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionPointsBreakdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BasePoints",
                table: "Predictions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayerGoalPoints",
                table: "Predictions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamBonusPoints",
                table: "Predictions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePoints",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "PlayerGoalPoints",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "TeamBonusPoints",
                table: "Predictions");
        }
    }
}
