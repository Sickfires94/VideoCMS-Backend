using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLogUserIdToName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "VideoMetadataChangeLogs");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserName",
                table: "VideoMetadataChangeLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedByUserName",
                table: "VideoMetadataChangeLogs");

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "VideoMetadataChangeLogs",
                type: "int",
                nullable: true);
        }
    }
}
