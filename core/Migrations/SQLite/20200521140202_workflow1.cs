using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace puck.core.Migrations.SQLite
{
    public partial class workflow1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PuckUserGroups",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PuckWorkflowItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentId = table.Column<Guid>(nullable: false),
                    Variant = table.Column<string>(maxLength: 10, nullable: true),
                    Status = table.Column<string>(maxLength: 256, nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Group = table.Column<string>(nullable: true),
                    Assignees = table.Column<string>(nullable: true),
                    LockedBy = table.Column<string>(maxLength: 256, nullable: true),
                    LockedUntil = table.Column<DateTime>(nullable: true),
                    Complete = table.Column<bool>(nullable: false),
                    CompleteDate = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    ViewedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuckWorkflowItem", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuckWorkflowItem_Complete",
                table: "PuckWorkflowItem",
                column: "Complete");

            migrationBuilder.CreateIndex(
                name: "IX_PuckWorkflowItem_CompleteDate",
                table: "PuckWorkflowItem",
                column: "CompleteDate");

            migrationBuilder.CreateIndex(
                name: "IX_PuckWorkflowItem_ContentId",
                table: "PuckWorkflowItem",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_PuckWorkflowItem_Group",
                table: "PuckWorkflowItem",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_PuckWorkflowItem_LockedBy",
                table: "PuckWorkflowItem",
                column: "LockedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PuckWorkflowItem_LockedUntil",
                table: "PuckWorkflowItem",
                column: "LockedUntil");

            migrationBuilder.CreateIndex(
                name: "IX_PuckWorkflowItem_Status",
                table: "PuckWorkflowItem",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PuckWorkflowItem_Timestamp",
                table: "PuckWorkflowItem",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PuckWorkflowItem_Variant",
                table: "PuckWorkflowItem",
                column: "Variant");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuckWorkflowItem");

            migrationBuilder.DropColumn(
                name: "PuckUserGroups",
                table: "AspNetUsers");
        }
    }
}
