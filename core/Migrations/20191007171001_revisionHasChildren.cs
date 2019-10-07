using Microsoft.EntityFrameworkCore.Migrations;

namespace puck.core.Migrations
{
    public partial class revisionHasChildren : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasChildren",
                table: "PuckRevision",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasChildren",
                table: "PuckRevision");
        }
    }
}
