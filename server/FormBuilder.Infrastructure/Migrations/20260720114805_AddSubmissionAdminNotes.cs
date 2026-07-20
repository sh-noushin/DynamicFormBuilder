using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionAdminNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "FormSubmissions",
                type: "TEXT",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "FormSubmissions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaviconUrl",
                table: "Forms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "Forms",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OneResponsePerIp",
                table: "Forms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WebhookSlackFormat",
                table: "Forms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KeyPrefix = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    KeyHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormSubmissionDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FormId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResumeToken = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubmitterName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SubmitterEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    FieldValuesJson = table.Column<string>(type: "TEXT", maxLength: 64000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormSubmissionDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormSubmissionDrafts_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_KeyHash",
                table: "ApiKeys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissionDrafts_ExpiresAt",
                table: "FormSubmissionDrafts",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissionDrafts_FormId",
                table: "FormSubmissionDrafts",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissionDrafts_ResumeToken",
                table: "FormSubmissionDrafts",
                column: "ResumeToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "FormSubmissionDrafts");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "FaviconUrl",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "OneResponsePerIp",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "WebhookSlackFormat",
                table: "Forms");
        }
    }
}
