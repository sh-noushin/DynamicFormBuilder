using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FormBuilder.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Forms",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("2daa2bec-d937-44e1-8b3e-9c2331acc94b"), "Survey Form 2" },
                    { new Guid("332ce45d-814e-46d3-8e1a-a129ab63b58d"), "Survey Form 1" },
                    { new Guid("ef1ac28c-b2b4-4a1e-bc99-7ad17b0c5461"), "Survey Form 3" }
                });

            migrationBuilder.InsertData(
                table: "FormVersions",
                columns: new[] { "Id", "CreatedAt", "FormId", "Version" },
                values: new object[,]
                {
                    { new Guid("309f9fe8-1975-4526-821d-e13abb8ad0dc"), new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9117), new Guid("332ce45d-814e-46d3-8e1a-a129ab63b58d"), "v2.0" },
                    { new Guid("488cc030-a7a9-4135-81b1-cd4338fddf0c"), new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9130), new Guid("ef1ac28c-b2b4-4a1e-bc99-7ad17b0c5461"), "v2.0" },
                    { new Guid("a0727ace-3630-4c6c-8583-432df14cffc9"), new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9125), new Guid("2daa2bec-d937-44e1-8b3e-9c2331acc94b"), "v2.0" },
                    { new Guid("cc3d790a-da55-47db-98b7-75dac4dd5de5"), new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9121), new Guid("2daa2bec-d937-44e1-8b3e-9c2331acc94b"), "v1.0" },
                    { new Guid("d372874d-577c-49d9-ae98-cb445e6a7b9c"), new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(8889), new Guid("332ce45d-814e-46d3-8e1a-a129ab63b58d"), "v1.0" },
                    { new Guid("f3b7fa94-1bef-4f8d-835d-73865f07c77f"), new DateTime(2025, 7, 4, 18, 15, 41, 689, DateTimeKind.Utc).AddTicks(9128), new Guid("ef1ac28c-b2b4-4a1e-bc99-7ad17b0c5461"), "v1.0" }
                });

            migrationBuilder.InsertData(
                table: "FormControls",
                columns: new[] { "Id", "FormVersionId", "IsRequired", "Label", "Type" },
                values: new object[,]
                {
                    { new Guid("aaa111aa-0000-0000-0000-000000000001"), new Guid("d372874d-577c-49d9-ae98-cb445e6a7b9c"), true, "Name", 0 },
                    { new Guid("aaa111aa-0000-0000-0000-000000000002"), new Guid("d372874d-577c-49d9-ae98-cb445e6a7b9c"), true, "Email", 3 },
                    { new Guid("aaa111aa-0000-0000-0000-000000000003"), new Guid("d372874d-577c-49d9-ae98-cb445e6a7b9c"), false, "Gender", 7 }
                });

            migrationBuilder.InsertData(
                table: "FormSubmissions",
                columns: new[] { "Id", "FormVersionId", "SubmittedAt" },
                values: new object[] { new Guid("ddd111aa-0000-0000-0000-000000000001"), new Guid("d372874d-577c-49d9-ae98-cb445e6a7b9c"), new DateTime(2025, 7, 4, 18, 15, 41, 690, DateTimeKind.Utc).AddTicks(2369) });

            migrationBuilder.InsertData(
                table: "FormControlValues",
                columns: new[] { "Id", "FormControlId", "Value" },
                values: new object[,]
                {
                    { new Guid("ccc111aa-0000-0000-0000-000000000001"), new Guid("aaa111aa-0000-0000-0000-000000000003"), "Male" },
                    { new Guid("ccc111aa-0000-0000-0000-000000000002"), new Guid("aaa111aa-0000-0000-0000-000000000003"), "Female" }
                });

            migrationBuilder.InsertData(
                table: "FormSubmissionValues",
                columns: new[] { "Id", "FormControlId", "FormSubmissionId", "Value" },
                values: new object[,]
                {
                    { new Guid("eee111aa-0000-0000-0000-000000000001"), new Guid("aaa111aa-0000-0000-0000-000000000001"), new Guid("ddd111aa-0000-0000-0000-000000000001"), "John Doe" },
                    { new Guid("eee111aa-0000-0000-0000-000000000002"), new Guid("aaa111aa-0000-0000-0000-000000000002"), new Guid("ddd111aa-0000-0000-0000-000000000001"), "john@example.com" },
                    { new Guid("eee111aa-0000-0000-0000-000000000003"), new Guid("aaa111aa-0000-0000-0000-000000000003"), new Guid("ddd111aa-0000-0000-0000-000000000001"), "Male" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FormControlValues",
                keyColumn: "Id",
                keyValue: new Guid("ccc111aa-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "FormControlValues",
                keyColumn: "Id",
                keyValue: new Guid("ccc111aa-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "FormSubmissionValues",
                keyColumn: "Id",
                keyValue: new Guid("eee111aa-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "FormSubmissionValues",
                keyColumn: "Id",
                keyValue: new Guid("eee111aa-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "FormSubmissionValues",
                keyColumn: "Id",
                keyValue: new Guid("eee111aa-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("309f9fe8-1975-4526-821d-e13abb8ad0dc"));

            migrationBuilder.DeleteData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("488cc030-a7a9-4135-81b1-cd4338fddf0c"));

            migrationBuilder.DeleteData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("a0727ace-3630-4c6c-8583-432df14cffc9"));

            migrationBuilder.DeleteData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("cc3d790a-da55-47db-98b7-75dac4dd5de5"));

            migrationBuilder.DeleteData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("f3b7fa94-1bef-4f8d-835d-73865f07c77f"));

            migrationBuilder.DeleteData(
                table: "FormControls",
                keyColumn: "Id",
                keyValue: new Guid("aaa111aa-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "FormControls",
                keyColumn: "Id",
                keyValue: new Guid("aaa111aa-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "FormControls",
                keyColumn: "Id",
                keyValue: new Guid("aaa111aa-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "FormSubmissions",
                keyColumn: "Id",
                keyValue: new Guid("ddd111aa-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Forms",
                keyColumn: "Id",
                keyValue: new Guid("2daa2bec-d937-44e1-8b3e-9c2331acc94b"));

            migrationBuilder.DeleteData(
                table: "Forms",
                keyColumn: "Id",
                keyValue: new Guid("ef1ac28c-b2b4-4a1e-bc99-7ad17b0c5461"));

            migrationBuilder.DeleteData(
                table: "FormVersions",
                keyColumn: "Id",
                keyValue: new Guid("d372874d-577c-49d9-ae98-cb445e6a7b9c"));

            migrationBuilder.DeleteData(
                table: "Forms",
                keyColumn: "Id",
                keyValue: new Guid("332ce45d-814e-46d3-8e1a-a129ab63b58d"));
        }
    }
}
