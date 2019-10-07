using Microsoft.EntityFrameworkCore.Migrations;

namespace puck.core.Migrations
{
    public partial class revisionIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PuckRevision_Current",
                table: "PuckRevision",
                column: "Current");

            migrationBuilder.CreateIndex(
                name: "IX_PuckRevision_HasNoPublishedRevision",
                table: "PuckRevision",
                column: "HasNoPublishedRevision");

            migrationBuilder.CreateIndex(
                name: "IX_PuckRevision_Id",
                table: "PuckRevision",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PuckRevision_ParentId",
                table: "PuckRevision",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PuckRevision_Variant",
                table: "PuckRevision",
                column: "Variant");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PuckRevision_Current",
                table: "PuckRevision");

            migrationBuilder.DropIndex(
                name: "IX_PuckRevision_HasNoPublishedRevision",
                table: "PuckRevision");

            migrationBuilder.DropIndex(
                name: "IX_PuckRevision_Id",
                table: "PuckRevision");

            migrationBuilder.DropIndex(
                name: "IX_PuckRevision_ParentId",
                table: "PuckRevision");

            migrationBuilder.DropIndex(
                name: "IX_PuckRevision_Variant",
                table: "PuckRevision");
        }
    }
}
