using Microsoft.EntityFrameworkCore.Migrations;

namespace puck.core.Migrations
{
    public partial class puckMetaIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PuckMeta_Key",
                table: "PuckMeta",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_PuckMeta_Name",
                table: "PuckMeta",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PuckMeta_Key",
                table: "PuckMeta");

            migrationBuilder.DropIndex(
                name: "IX_PuckMeta_Name",
                table: "PuckMeta");
        }
    }
}
