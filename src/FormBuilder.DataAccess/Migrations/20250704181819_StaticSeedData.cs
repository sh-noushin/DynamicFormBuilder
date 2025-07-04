using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class StaticSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FormSubmissions",
                keyColumn: "Id",
                keyValue: new Guid("ddd111aa-0000-0000-0000-000000000001"),
                column: "SubmittedAt",
                value: new DateTime(2025, 7, 1, 16, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("309f9fe8-1975-4526-821d-e13abb8ad0dc"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 11, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("488cc030-a7a9-4135-81b1-cd4338fddf0c"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 15, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("a0727ace-3630-4c6c-8583-432df14cffc9"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 13, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("cc3d790a-da55-47db-98b7-75dac4dd5de5"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 12, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("d372874d-577c-49d9-ae98-cb445e6a7b9c"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 10, 0, 0, 0, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("f3b7fa94-1bef-4f8d-835d-73865f07c77f"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 1, 14, 0, 0, 0, DateTimeKind.Local));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FormSubmissions",
                keyColumn: "Id",
                keyValue: new Guid("ddd111aa-0000-0000-0000-000000000001"),
                column: "SubmittedAt",
                value: new DateTime(2025, 7, 4, 18, 15, 41, 690, DateTimeKind.Utc).AddTicks(2369));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("309f9fe8-1975-4526-821d-e13abb8ad0dc"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9117));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("488cc030-a7a9-4135-81b1-cd4338fddf0c"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9130));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("a0727ace-3630-4c6c-8583-432df14cffc9"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9125));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("cc3d790a-da55-47db-98b7-75dac4dd5de5"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9121));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("d372874d-577c-49d9-ae98-cb445e6a7b9c"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(8889));

            migrationBuilder.UpdateData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("f3b7fa94-1bef-4f8d-835d-73865f07c77f"),
                column: "CreatedAt",
                value: new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9128));
        }
    }
}
