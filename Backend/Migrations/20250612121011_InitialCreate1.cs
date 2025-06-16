using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tags_videoMetadata_VideoMetadatavideoId",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "IX_tags_VideoMetadatavideoId",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "VideoMetadatavideoId",
                table: "tags");

            migrationBuilder.CreateTable(
                name: "TagVideoMetadata",
                columns: table => new
                {
                    VideoMetadatavideoId = table.Column<int>(type: "int", nullable: false),
                    videoTagstagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagVideoMetadata", x => new { x.VideoMetadatavideoId, x.videoTagstagId });
                    table.ForeignKey(
                        name: "FK_TagVideoMetadata_tags_videoTagstagId",
                        column: x => x.videoTagstagId,
                        principalTable: "tags",
                        principalColumn: "tagId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagVideoMetadata_videoMetadata_VideoMetadatavideoId",
                        column: x => x.VideoMetadatavideoId,
                        principalTable: "videoMetadata",
                        principalColumn: "videoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagVideoMetadata_videoTagstagId",
                table: "TagVideoMetadata",
                column: "videoTagstagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagVideoMetadata");

            migrationBuilder.AddColumn<int>(
                name: "VideoMetadatavideoId",
                table: "tags",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_VideoMetadatavideoId",
                table: "tags",
                column: "VideoMetadatavideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_tags_videoMetadata_VideoMetadatavideoId",
                table: "tags",
                column: "VideoMetadatavideoId",
                principalTable: "videoMetadata",
                principalColumn: "videoId");
        }
    }
}
