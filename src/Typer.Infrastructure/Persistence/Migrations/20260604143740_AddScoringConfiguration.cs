using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Typer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoringConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CorrectWinnerPoints = table.Column<int>(type: "integer", nullable: false),
                    CorrectGoalDifferenceBonus = table.Column<int>(type: "integer", nullable: false),
                    ExactScorePoints = table.Column<int>(type: "integer", nullable: false),
                    FavoriteTeamMultiplier = table.Column<double>(type: "double precision", nullable: false),
                    TournamentWinnerBonus = table.Column<int>(type: "integer", nullable: false),
                    FavoritePlayerGoalBonus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringConfigurations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ScoringConfigurations",
                columns: new[] { "Id", "CorrectGoalDifferenceBonus", "CorrectWinnerPoints", "CreatedAt", "ExactScorePoints", "FavoritePlayerGoalBonus", "FavoriteTeamMultiplier", "IsActive", "Name", "TournamentWinnerBonus", "UpdatedAt" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), 1, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 3, 2.0, true, "Domyślna", 20, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScoringConfigurations");
        }
    }
}
