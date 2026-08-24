using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace git_repo_dashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Projects",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Projects");
        }
    }
}
