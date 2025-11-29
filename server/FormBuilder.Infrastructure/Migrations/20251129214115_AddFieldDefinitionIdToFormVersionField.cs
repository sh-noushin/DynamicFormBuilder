using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldDefinitionIdToFormVersionField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Column already exists in some environments (hotfix applied directly to DB).
            // No-op here to avoid errors when the column is present.
            // If you prefer the migration to actually create the column, replace this block
            // with the AddColumn call generated originally.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: column removal is not performed to avoid data loss in environments
            // where the column was added manually or is in use.
        }
    }
}
