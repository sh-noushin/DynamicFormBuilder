using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFormSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Forms",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            // Backfill unique 8-char slugs for any pre-existing forms so the
            // unique index below does not fail on empty defaults.
            migrationBuilder.Sql("UPDATE Forms SET Slug = substr(hex(randomblob(4)), 1, 8) WHERE Slug = '';");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_Slug",
                table: "Forms",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Forms_Slug",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Forms");
        }
    }
}
