using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class updatedVideoMetaDataName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TagVideoMetadata_videoMetadata_VideoMetadatavideoId",
                table: "TagVideoMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_videoMetadata_categories_categoryId",
                table: "videoMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_videoMetadata_users_userId",
                table: "videoMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_videoMetadata",
                table: "videoMetadata");

            migrationBuilder.RenameTable(
                name: "videoMetadata",
                newName: "videoMetadatas");

            migrationBuilder.RenameIndex(
                name: "IX_videoMetadata_userId",
                table: "videoMetadatas",
                newName: "IX_videoMetadatas_userId");

            migrationBuilder.RenameIndex(
                name: "IX_videoMetadata_categoryId",
                table: "videoMetadatas",
                newName: "IX_videoMetadatas_categoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_videoMetadatas",
                table: "videoMetadatas",
                column: "videoId");

            migrationBuilder.AddForeignKey(
                name: "FK_TagVideoMetadata_videoMetadatas_VideoMetadatavideoId",
                table: "TagVideoMetadata",
                column: "VideoMetadatavideoId",
                principalTable: "videoMetadatas",
                principalColumn: "videoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_videoMetadatas_categories_categoryId",
                table: "videoMetadatas",
                column: "categoryId",
                principalTable: "categories",
                principalColumn: "categoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_videoMetadatas_users_userId",
                table: "videoMetadatas",
                column: "userId",
                principalTable: "users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TagVideoMetadata_videoMetadatas_VideoMetadatavideoId",
                table: "TagVideoMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_videoMetadatas_categories_categoryId",
                table: "videoMetadatas");

            migrationBuilder.DropForeignKey(
                name: "FK_videoMetadatas_users_userId",
                table: "videoMetadatas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_videoMetadatas",
                table: "videoMetadatas");

            migrationBuilder.RenameTable(
                name: "videoMetadatas",
                newName: "videoMetadata");

            migrationBuilder.RenameIndex(
                name: "IX_videoMetadatas_userId",
                table: "videoMetadata",
                newName: "IX_videoMetadata_userId");

            migrationBuilder.RenameIndex(
                name: "IX_videoMetadatas_categoryId",
                table: "videoMetadata",
                newName: "IX_videoMetadata_categoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_videoMetadata",
                table: "videoMetadata",
                column: "videoId");

            migrationBuilder.AddForeignKey(
                name: "FK_TagVideoMetadata_videoMetadata_VideoMetadatavideoId",
                table: "TagVideoMetadata",
                column: "VideoMetadatavideoId",
                principalTable: "videoMetadata",
                principalColumn: "videoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_videoMetadata_categories_categoryId",
                table: "videoMetadata",
                column: "categoryId",
                principalTable: "categories",
                principalColumn: "categoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_videoMetadata_users_userId",
                table: "videoMetadata",
                column: "userId",
                principalTable: "users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
