using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingFieldsToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Plan",
                table: "Organizations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Organizations",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "Organizations",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionCurrentPeriodEnd",
                table: "Organizations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionStatus",
                table: "Organizations",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_StripeCustomerId",
                table: "Organizations",
                column: "StripeCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_StripeSubscriptionId",
                table: "Organizations",
                column: "StripeSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organizations_StripeCustomerId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_StripeSubscriptionId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "SubscriptionCurrentPeriodEnd",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "SubscriptionStatus",
                table: "Organizations");
        }
    }
}
