using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Typer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixYamalPlayerPhotoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Players"
                SET "PhotoUrl" = 'https://commons.wikimedia.org/wiki/Special:FilePath/Lamine_Yamal_in_2025_(cropped).jpg?width=200'
                WHERE "FirstName" = 'Lamine' AND "LastName" = 'Yamal';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
