using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    categoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    categoryParentIdcategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.categoryId);
                    table.ForeignKey(
                        name: "FK_categories_categories_categoryParentIdcategoryId",
                        column: x => x.categoryParentIdcategoryId,
                        principalTable: "categories",
                        principalColumn: "categoryId");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userCreatedDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "getdate()"),
                    userUpdatedDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "getDate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.userId);
                });

            migrationBuilder.CreateTable(
                name: "videoMetadata",
                columns: table => new
                {
                    videoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    videoName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    videoDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    videoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    categoryId = table.Column<int>(type: "int", nullable: true),
                    userId = table.Column<int>(type: "int", nullable: false),
                    videoUploadDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "getdate()"),
                    videoUpdatedDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_videoMetadata", x => x.videoId);
                    table.ForeignKey(
                        name: "FK_videoMetadata_categories_categoryId",
                        column: x => x.categoryId,
                        principalTable: "categories",
                        principalColumn: "categoryId");
                    table.ForeignKey(
                        name: "FK_videoMetadata_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    tagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tagName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VideoMetadatavideoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.tagId);
                    table.ForeignKey(
                        name: "FK_tags_videoMetadata_VideoMetadatavideoId",
                        column: x => x.VideoMetadatavideoId,
                        principalTable: "videoMetadata",
                        principalColumn: "videoId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_categoryParentIdcategoryId",
                table: "categories",
                column: "categoryParentIdcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_VideoMetadatavideoId",
                table: "tags",
                column: "VideoMetadatavideoId");

            migrationBuilder.CreateIndex(
                name: "IX_videoMetadata_categoryId",
                table: "videoMetadata",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_videoMetadata_userId",
                table: "videoMetadata",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "videoMetadata");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
