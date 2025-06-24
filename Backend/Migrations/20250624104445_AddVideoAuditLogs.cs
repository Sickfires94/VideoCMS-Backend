using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VideoMetadataChangeLogs",
                columns: table => new
                {
                    VideoId = table.Column<int>(type: "int", nullable: false),
                    ChangeTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreviousVideoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedVideoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousVideoDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedVideoDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousVideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedVideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousCategoryId = table.Column<int>(type: "int", nullable: true),
                    UpdatedCategoryId = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoMetadataChangeLogs", x => new { x.VideoId, x.ChangeTime });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoMetadataChangeLogs");
        }
    }
}
