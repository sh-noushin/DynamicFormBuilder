using FormBuilder.Infrastructure.Data;
using FormBuilder.Infrastructure.Repositories;
using FormBuilder.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Tests.Repositories
{
    public class FormSubmissionRepositoryTests
    {
        private FormBuilderDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<FormBuilderDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FormBuilderDbContext(options);
        }

        private FormVersion CreateFormVersion(FormBuilderDbContext context)
        {
            var form = new Form { Id = Guid.NewGuid(), Name = "Form", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var version = new FormVersion { Id = Guid.NewGuid(), FormId = form.Id, VersionNumber = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, Form = form };
            context.Forms.Add(form);
            context.FormVersions.Add(version);
            context.SaveChanges();
            return version;
        }

        [Fact]
        public async Task CreateAsync_AddsSubmissionToDatabase()
        {
            var context = GetDbContext();
            var version = CreateFormVersion(context);
            var repo = new FormSubmissionRepository(context);
            var submission = new FormSubmission
            {
                Id = Guid.NewGuid(),
                FormVersionId = version.Id,
                SubmittedAt = DateTime.UtcNow,
                SubmitterName = "Tester",
                SubmitterEmail = "test@example.com",
                Values = new List<FormSubmissionValue> {
                    new FormSubmissionValue { FieldName = "Field1", FieldValue = "Value1" }
                }
            };
            await repo.CreateAsync(submission);
            var result = await context.FormSubmissions.FindAsync(submission.Id);
            Assert.NotNull(result);
            Assert.Equal("Tester", result.SubmitterName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectSubmission()
        {
            var context = GetDbContext();
            var version = CreateFormVersion(context);
            var submission = new FormSubmission
            {
                Id = Guid.NewGuid(),
                FormVersionId = version.Id,
                SubmittedAt = DateTime.UtcNow,
                SubmitterName = "FindMe",
                Values = new List<FormSubmissionValue> {
                    new FormSubmissionValue { FieldName = "Field1", FieldValue = "Value1" }
                }
            };
            context.FormSubmissions.Add(submission);
            await context.SaveChangesAsync();
            var repo = new FormSubmissionRepository(context);
            var result = await repo.GetByIdAsync(submission.Id);
            Assert.NotNull(result);
            Assert.Equal("FindMe", result.SubmitterName);
            Assert.Single(result.Values);
        }

        [Fact]
        public async Task GetByFormVersionIdAsync_ReturnsSubmissions()
        {
            var context = GetDbContext();
            var version = CreateFormVersion(context);
            context.FormSubmissions.Add(new FormSubmission { Id = Guid.NewGuid(), FormVersionId = version.Id, SubmittedAt = DateTime.UtcNow });
            context.FormSubmissions.Add(new FormSubmission { Id = Guid.NewGuid(), FormVersionId = version.Id, SubmittedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormSubmissionRepository(context);
            var submissions = await repo.GetByFormVersionIdAsync(version.Id);
            Assert.Equal(2, new List<FormSubmission>(submissions).Count);
        }

        [Fact]
        public async Task GetSubmissionCountByFormVersionIdAsync_ReturnsCount()
        {
            var context = GetDbContext();
            var version = CreateFormVersion(context);
            context.FormSubmissions.Add(new FormSubmission { Id = Guid.NewGuid(), FormVersionId = version.Id, SubmittedAt = DateTime.UtcNow });
            context.FormSubmissions.Add(new FormSubmission { Id = Guid.NewGuid(), FormVersionId = version.Id, SubmittedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormSubmissionRepository(context);
            var count = await repo.GetSubmissionCountByFormVersionIdAsync(version.Id);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesSubmission()
        {
            var context = GetDbContext();
            var version = CreateFormVersion(context);
            var submission = new FormSubmission
            {
                Id = Guid.NewGuid(),
                FormVersionId = version.Id,
                SubmittedAt = DateTime.UtcNow,
                SubmitterName = "Old",
                SubmitterEmail = "old@example.com",
                Values = new List<FormSubmissionValue> {
                    new FormSubmissionValue { FieldName = "Field1", FieldValue = "Value1" }
                }
            };
            context.FormSubmissions.Add(submission);
            await context.SaveChangesAsync();
            var repo = new FormSubmissionRepository(context);
            var source = new FormSubmission
            {
                SubmitterName = "New",
                SubmitterEmail = "new@example.com",
                Values = new List<FormSubmissionValue>
                {
                    new FormSubmissionValue { FieldName = "Field2", FieldValue = "Value2" }
                }
            };
            var updated = await repo.UpdateAsync(submission.Id, source);
            Assert.NotNull(updated);
            Assert.Equal("New", updated!.SubmitterName);
            Assert.Equal("new@example.com", updated.SubmitterEmail);
            Assert.Single(updated.Values);
            Assert.Equal("Field2", updated.Values[0].FieldName);
            Assert.Equal("Value2", updated.Values[0].FieldValue);
        }

        [Fact]
        public async Task DeleteAsync_RemovesSubmission()
        {
            var context = GetDbContext();
            var version = CreateFormVersion(context);
            var submission = new FormSubmission { Id = Guid.NewGuid(), FormVersionId = version.Id, SubmittedAt = DateTime.UtcNow };
            context.FormSubmissions.Add(submission);
            await context.SaveChangesAsync();
            var repo = new FormSubmissionRepository(context);
            var deleted = await repo.DeleteAsync(submission.Id);
            Assert.True(deleted);
            var found = await context.FormSubmissions.FindAsync(submission.Id);
            Assert.Null(found);
        }
    }
}
