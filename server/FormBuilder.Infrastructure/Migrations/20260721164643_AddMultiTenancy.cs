using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Forms",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "ApiKeys",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_OrganizationId",
                table: "Forms",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_OrganizationId",
                table: "ApiKeys",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Slug",
                table: "Organizations",
                column: "Slug",
                unique: true);

            // Backfill: create a "Default Workspace" org and assign every
            // pre-existing Form / User / ApiKey to it, so migrating an
            // existing DB doesn't strand rows with a Guid.Empty tenant.
            // A fixed sentinel Id keeps this deterministic across re-runs
            // in dev; the seeder's `Slug == "default"` lookup finds this
            // exact row on next startup and doesn't create a duplicate.
            const string DefaultOrgId = "11111111-1111-1111-1111-111111111111";
            migrationBuilder.Sql($@"
                INSERT INTO Organizations (Id, Name, Slug, CreatedAt)
                VALUES ('{DefaultOrgId}', 'Default Workspace', 'default', CURRENT_TIMESTAMP);
            ");
            migrationBuilder.Sql($"UPDATE Forms SET OrganizationId = '{DefaultOrgId}' WHERE OrganizationId = '00000000-0000-0000-0000-000000000000';");
            migrationBuilder.Sql($"UPDATE AspNetUsers SET OrganizationId = '{DefaultOrgId}' WHERE OrganizationId = '00000000-0000-0000-0000-000000000000';");
            migrationBuilder.Sql($"UPDATE ApiKeys SET OrganizationId = '{DefaultOrgId}' WHERE OrganizationId = '00000000-0000-0000-0000-000000000000';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Forms_OrganizationId",
                table: "Forms");

            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_OrganizationId",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ApiKeys");
        }
    }
}
