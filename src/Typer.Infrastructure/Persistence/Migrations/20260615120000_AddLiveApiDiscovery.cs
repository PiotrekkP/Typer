using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Typer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveApiDiscovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'Matches'
                      AND column_name = 'ApiFootballFixtureId'
                  ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'Matches'
                      AND column_name = 'LiveApiFixtureId'
                  ) THEN
                    ALTER TABLE "Matches" RENAME COLUMN "ApiFootballFixtureId" TO "LiveApiFixtureId";
                  END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF EXISTS (
                    SELECT 1 FROM pg_indexes
                    WHERE schemaname = 'public'
                      AND indexname = 'IX_Matches_ApiFootballFixtureId'
                  ) THEN
                    ALTER INDEX "IX_Matches_ApiFootballFixtureId" RENAME TO "IX_Matches_LiveApiFixtureId";
                  END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Matches" ADD COLUMN IF NOT EXISTS "LiveApiFixtureId" integer;
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_Matches_LiveApiFixtureId" ON "Matches" ("LiveApiFixtureId");
                """);

            migrationBuilder.AddColumn<int>(
                name: "LiveApiDiscoveryAttempts",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LiveApiId",
                table: "Teams",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LiveApiId",
                table: "Teams",
                column: "LiveApiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_LiveApiId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "LiveApiId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "LiveApiDiscoveryAttempts",
                table: "Matches");

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                  IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'Matches'
                      AND column_name = 'LiveApiFixtureId'
                  ) AND NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'Matches'
                      AND column_name = 'ApiFootballFixtureId'
                  ) THEN
                    ALTER TABLE "Matches" RENAME COLUMN "LiveApiFixtureId" TO "ApiFootballFixtureId";
                  END IF;
                END $$;
                """);
        }
    }
}
