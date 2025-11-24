using FormBuilder.Infrastructure.Data;
using FormBuilder.Infrastructure.Repositories;
using FormBuilder.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Tests.Repositories
{
    public class FormVersionRepositoryTests
    {
        private FormBuilderDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<FormBuilderDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FormBuilderDbContext(options);
        }

        private Form CreateForm(FormBuilderDbContext context)
        {
            var form = new Form { Id = Guid.NewGuid(), Name = "Form", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Forms.Add(form);
            context.SaveChanges();
            return form;
        }

        [Fact]
        public async Task CreateAsync_AddsVersionToDatabase()
        {
            var context = GetDbContext();
            var form = CreateForm(context);
            var repo = new FormVersionRepository(context);
            var version = new FormVersion
            {
                Id = Guid.NewGuid(),
                FormId = form.Id,
                VersionNumber = 1,
                Description = "desc",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPublished = false,
                IsCurrentVersion = true
            };
            await repo.CreateAsync(version);
            var result = await context.FormVersions.FindAsync(version.Id);
            Assert.NotNull(result);
            Assert.Equal("desc", result.Description);
        }

        [Fact]
        public async Task GetVersionsByFormIdAsync_ReturnsVersions()
        {
            var context = GetDbContext();
            var form = CreateForm(context);
            context.FormVersions.Add(new FormVersion { Id = Guid.NewGuid(), FormId = form.Id, VersionNumber = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            context.FormVersions.Add(new FormVersion { Id = Guid.NewGuid(), FormId = form.Id, VersionNumber = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormVersionRepository(context);
            var versions = await repo.GetVersionsByFormIdAsync(form.Id);
            Assert.Equal(2, new List<FormVersion>(versions).Count);
        }

        [Fact]
        public async Task GetVersionAsync_ReturnsCorrectVersion()
        {
            var context = GetDbContext();
            var form = CreateForm(context);
            context.FormVersions.Add(new FormVersion { Id = Guid.NewGuid(), FormId = form.Id, VersionNumber = 3, Description = "FindMe", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormVersionRepository(context);
            var version = await repo.GetVersionAsync(form.Id, 3);
            Assert.NotNull(version);
            Assert.Equal("FindMe", version.Description);
        }

        [Fact]
        public async Task GetVersionByIdAsync_ReturnsCorrectVersion()
        {
            var context = GetDbContext();
            var form = CreateForm(context);
            var id = Guid.NewGuid();
            context.FormVersions.Add(new FormVersion { Id = id, FormId = form.Id, VersionNumber = 4, Description = "ById", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormVersionRepository(context);
            var version = await repo.GetVersionByIdAsync(id);
            Assert.NotNull(version);
            Assert.Equal("ById", version.Description);
        }

        [Fact]
        public async Task GetCurrentVersionAsync_ReturnsCurrentVersion()
        {
            var context = GetDbContext();
            var form = CreateForm(context);
            context.FormVersions.Add(new FormVersion { Id = Guid.NewGuid(), FormId = form.Id, VersionNumber = 5, IsCurrentVersion = true, Description = "Current", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormVersionRepository(context);
            var version = await repo.GetCurrentVersionAsync(form.Id);
            Assert.NotNull(version);
            Assert.Equal("Current", version.Description);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesVersion()
        {
            var context = GetDbContext();
            var form = CreateForm(context);
            context.FormVersions.Add(new FormVersion { Id = Guid.NewGuid(), FormId = form.Id, VersionNumber = 6, Description = "Old", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsPublished = false, IsCurrentVersion = false });
            await context.SaveChangesAsync();
            var repo = new FormVersionRepository(context);
            var updated = new FormVersion { Description = "New", IsPublished = true, IsCurrentVersion = true, UpdatedAt = DateTime.UtcNow };
            var result = await repo.UpdateAsync(form.Id, 6, updated);
            Assert.NotNull(result);
            Assert.Equal("New", result.Description);
            Assert.True(result.IsPublished);
            Assert.True(result.IsCurrentVersion);
        }

        [Fact]
        public async Task DeleteAsync_RemovesVersion()
        {
            var context = GetDbContext();
            var form = CreateForm(context);
            context.FormVersions.Add(new FormVersion { Id = Guid.NewGuid(), FormId = form.Id, VersionNumber = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormVersionRepository(context);
            var deleted = await repo.DeleteAsync(form.Id, 7);
            Assert.True(deleted);
            var found = await context.FormVersions.FirstOrDefaultAsync(v => v.FormId == form.Id && v.VersionNumber == 7);
            Assert.Null(found);
        }
    }
}
