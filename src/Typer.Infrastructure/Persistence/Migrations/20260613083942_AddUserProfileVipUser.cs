using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Typer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileVipUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VipUser",
                table: "UserProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_VipUser",
                table: "UserProfiles",
                column: "VipUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_VipUser",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "VipUser",
                table: "UserProfiles");
        }
    }
}
