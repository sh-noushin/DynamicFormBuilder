using FormBuilder.Infrastructure.Data;
using FormBuilder.Infrastructure.Repositories;
using FormBuilder.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Tests.Repositories
{
    public class FormRepositoryTests
    {
        private FormBuilderDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<FormBuilderDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FormBuilderDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsFormToDatabase()
        {
            var context = GetDbContext();
            var repo = new FormRepository(context);
            var form = new Form
            {
                Id = Guid.NewGuid(),
                Name = "Test Form",
                Description = "A test form",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await repo.CreateAsync(form);
            var result = await context.Forms.FindAsync(form.Id);
            Assert.NotNull(result);
            Assert.Equal("Test Form", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectForm()
        {
            var context = GetDbContext();
            var id = Guid.NewGuid();
            context.Forms.Add(new Form { Id = id, Name = "FindMe", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormRepository(context);
            var form = await repo.GetByIdAsync(id);
            Assert.NotNull(form);
            Assert.Equal("FindMe", form.Name);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesForm()
        {
            var context = GetDbContext();
            var id = Guid.NewGuid();
            context.Forms.Add(new Form { Id = id, Name = "Old", Description = "desc", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = false });
            await context.SaveChangesAsync();
            var repo = new FormRepository(context);
            var updated = new Form { Name = "New", Description = "new desc", IsActive = true };
            var result = await repo.UpdateAsync(id, updated);
            Assert.NotNull(result);
            Assert.Equal("New", result.Name);
            Assert.Equal("new desc", result.Description);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task DeleteAsync_RemovesForm()
        {
            var context = GetDbContext();
            var id = Guid.NewGuid();
            context.Forms.Add(new Form { Id = id, Name = "ToDelete", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormRepository(context);
            var deleted = await repo.DeleteAsync(id);
            Assert.True(deleted);
            var form = await context.Forms.FindAsync(id);
            Assert.Null(form);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllForms()
        {
            var context = GetDbContext();
            context.Forms.Add(new Form { Id = Guid.NewGuid(), Name = "Form1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            context.Forms.Add(new Form { Id = Guid.NewGuid(), Name = "Form2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var repo = new FormRepository(context);
            var forms = await repo.GetAllAsync();
            Assert.Equal(2, new List<Form>(forms).Count);
        }
    }
}
