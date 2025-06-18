using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_categoryParentIdcategoryId",
                table: "categories");

            migrationBuilder.RenameColumn(
                name: "categoryParentIdcategoryId",
                table: "categories",
                newName: "categoryParentId");

            migrationBuilder.RenameIndex(
                name: "IX_categories_categoryParentIdcategoryId",
                table: "categories",
                newName: "IX_categories_categoryParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_categoryParentId",
                table: "categories",
                column: "categoryParentId",
                principalTable: "categories",
                principalColumn: "categoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_categoryParentId",
                table: "categories");

            migrationBuilder.RenameColumn(
                name: "categoryParentId",
                table: "categories",
                newName: "categoryParentIdcategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_categories_categoryParentId",
                table: "categories",
                newName: "IX_categories_categoryParentIdcategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_categoryParentIdcategoryId",
                table: "categories",
                column: "categoryParentIdcategoryId",
                principalTable: "categories",
                principalColumn: "categoryId");
        }
    }
}
