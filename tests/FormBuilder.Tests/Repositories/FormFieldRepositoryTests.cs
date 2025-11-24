using FormBuilder.Infrastructure.Data;
using FormBuilder.Infrastructure.Repositories;
using FormBuilder.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Tests.Repositories
{
    public class FormFieldRepositoryTests
    {
        private FormBuilderDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<FormBuilderDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FormBuilderDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_AddsFieldToDatabase()
        {
            var context = GetDbContext();
            var repo = new FormFieldRepository(context);
            var field = new FormVersionField { Id = Guid.NewGuid(), Name = "Field1", Order = 1, FormVersionId = Guid.NewGuid() };
            await repo.CreateAsync(field);
            var result = await context.FormVersionFields.FindAsync(field.Id);
            Assert.NotNull(result);
            Assert.Equal("Field1", result.Name);
        }

        [Fact]
        public async Task GetFieldsByVersionIdAsync_ReturnsFields()
        {
            var context = GetDbContext();
            var versionId = Guid.NewGuid();
            context.FormVersionFields.Add(new FormVersionField { Id = Guid.NewGuid(), Name = "A", Order = 1, FormVersionId = versionId });
            context.FormVersionFields.Add(new FormVersionField { Id = Guid.NewGuid(), Name = "B", Order = 2, FormVersionId = versionId });
            await context.SaveChangesAsync();
            var repo = new FormFieldRepository(context);
            var fields = await repo.GetFieldsByVersionIdAsync(versionId);
            Assert.Equal(2, new List<FormVersionField>(fields).Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectField()
        {
            var context = GetDbContext();
            var id = Guid.NewGuid();
            context.FormVersionFields.Add(new FormVersionField { Id = id, Name = "FindMe", Order = 1, FormVersionId = Guid.NewGuid() });
            await context.SaveChangesAsync();
            var repo = new FormFieldRepository(context);
            var field = await repo.GetByIdAsync(id);
            Assert.NotNull(field);
            Assert.Equal("FindMe", field.Name);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesField()
        {
            var context = GetDbContext();
            var id = Guid.NewGuid();
            context.FormVersionFields.Add(new FormVersionField { Id = id, Name = "Old", Order = 1, FormVersionId = Guid.NewGuid() });
            await context.SaveChangesAsync();
            var repo = new FormFieldRepository(context);
            var updated = new FormVersionField { Name = "New", Label = "Label", Type = FieldType.Text, IsRequired = true, Order = 2, FormVersionId = Guid.NewGuid() };
            var result = await repo.UpdateAsync(id, updated);
            Assert.NotNull(result);
            Assert.Equal("New", result.Name);
            Assert.Equal("Label", result.Label);
            Assert.Equal(FieldType.Text, result.Type);
            Assert.True(result.IsRequired);
            Assert.Equal(2, result.Order);
        }

        [Fact]
        public async Task DeleteAsync_RemovesField()
        {
            var context = GetDbContext();
            var id = Guid.NewGuid();
            context.FormVersionFields.Add(new FormVersionField { Id = id, Name = "ToDelete", Order = 1, FormVersionId = Guid.NewGuid() });
            await context.SaveChangesAsync();
            var repo = new FormFieldRepository(context);
            var deleted = await repo.DeleteAsync(id);
            Assert.True(deleted);
            var field = await context.FormVersionFields.FindAsync(id);
            Assert.Null(field);
        }

        [Fact]
        public async Task BulkCreateAsync_AddsFields()
        {
            var context = GetDbContext();
            var repo = new FormFieldRepository(context);
            var fields = new List<FormVersionField>
            {
                new FormVersionField { Id = Guid.NewGuid(), Name = "Bulk1", Order = 1, FormVersionId = Guid.NewGuid() },
                new FormVersionField { Id = Guid.NewGuid(), Name = "Bulk2", Order = 2, FormVersionId = Guid.NewGuid() }
            };
            var result = await repo.BulkCreateAsync(fields);
            Assert.Equal(2, await context.FormVersionFields.CountAsync());
        }

        [Fact]
        public async Task BulkUpdateAsync_UpdatesFields()
        {
            var context = GetDbContext();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            context.FormVersionFields.Add(new FormVersionField { Id = id1, Name = "Old1", Order = 1, FormVersionId = Guid.NewGuid() });
            context.FormVersionFields.Add(new FormVersionField { Id = id2, Name = "Old2", Order = 2, FormVersionId = Guid.NewGuid() });
            await context.SaveChangesAsync();
            var repo = new FormFieldRepository(context);
            var updatedFields = new List<FormVersionField>
            {
                new FormVersionField { Id = id1, Name = "New1", Order = 1, FormVersionId = Guid.NewGuid() },
                new FormVersionField { Id = id2, Name = "New2", Order = 2, FormVersionId = Guid.NewGuid() }
            };
            var result = await repo.BulkUpdateAsync(updatedFields);
            Assert.True(result);
            var f1 = await context.FormVersionFields.FindAsync(id1);
            var f2 = await context.FormVersionFields.FindAsync(id2);
            Assert.NotNull(f1);
            Assert.NotNull(f2);
            Assert.Equal("New1", f1!.Name);
            Assert.Equal("New2", f2!.Name);
        }
    }
}
