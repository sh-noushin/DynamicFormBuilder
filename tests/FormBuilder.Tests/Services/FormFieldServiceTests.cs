using FormBuilder.Core.DTOs;
using FormBuilder.Core.Services;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using NSubstitute;

namespace FormBuilder.Tests.Services
{
    public class FormFieldServiceTests
    {
        private readonly IFormFieldRepository _repo = Substitute.For<IFormFieldRepository>();
        private readonly AutoMapper.IMapper _mapper = Substitute.For<AutoMapper.IMapper>();

        [Fact]
        public async Task GetFieldByIdAsync_ReturnsField()
        {
            var field = new FormVersionField { Id = Guid.NewGuid(), Name = "Field" };
            var fieldDto = new FormFieldDto { Id = field.Id, Name = field.Name };
            _repo.GetByIdAsync(field.Id).Returns(Task.FromResult<FormVersionField?>(field));
            _mapper.Map<FormFieldDto>(field).Returns(fieldDto);
            var service = new FormFieldService(_repo, _mapper);
            var result = await service.GetFieldByIdAsync(field.Id);
            Assert.NotNull(result);
            Assert.Equal("Field", result.Name);
        }

        [Fact]
        public async Task GetFieldsByVersionIdAsync_ReturnsFields()
        {
            var versionId = Guid.NewGuid();
            var fields = new List<FormVersionField> { new FormVersionField { Id = Guid.NewGuid(), Name = "A" } };
            var fieldDtos = new List<FormFieldDto> { new FormFieldDto { Id = fields[0].Id, Name = "A" } };
            _repo.GetFieldsByVersionIdAsync(versionId).Returns(fields);
            _mapper.Map<IEnumerable<FormFieldDto>>(fields).Returns(fieldDtos);
            var service = new FormFieldService(_repo, _mapper);
            var result = await service.GetFieldsByVersionIdAsync(versionId);
            Assert.Single(result);
            Assert.Equal("A", result.First().Name);
        }

        [Fact]
        public async Task CreateFieldAsync_CreatesField()
        {
            var createDto = new CreateFormFieldDto { Name = "NewField", FormVersionId = Guid.NewGuid() };
            var entity = new FormVersionField { Id = Guid.NewGuid(), Name = "NewField" };
            var resultDto = new FormFieldDto { Id = entity.Id, Name = entity.Name };
            _mapper.Map<FormVersionField>(createDto).Returns(entity);
            _repo.CreateAsync(entity).Returns(entity);
            _mapper.Map<FormFieldDto>(entity).Returns(resultDto);
            var service = new FormFieldService(_repo, _mapper);
            var result = await service.CreateFieldAsync(createDto);
            Assert.NotNull(result);
            Assert.Equal("NewField", result.Name);
        }

        [Fact]
        public async Task UpdateFieldAsync_UpdatesField()
        {
            var fieldId = Guid.NewGuid();
            var updateDto = new UpdateFormFieldDto { Name = "Updated", FormVersionId = Guid.NewGuid() };
            var entity = new FormVersionField { Id = fieldId, Name = "Updated" };
            var updatedEntity = new FormVersionField { Id = fieldId, Name = "Updated" };
            var resultDto = new FormFieldDto { Id = fieldId, Name = "Updated" };
            _mapper.Map<FormVersionField>(updateDto).Returns(entity);
            _repo.UpdateAsync(fieldId, entity).Returns(updatedEntity);
            _mapper.Map<FormFieldDto>(updatedEntity).Returns(resultDto);
            var service = new FormFieldService(_repo, _mapper);
            var result = await service.UpdateFieldAsync(fieldId, updateDto);
            Assert.NotNull(result);
            Assert.Equal("Updated", result.Name);
        }

        [Fact]
        public async Task DeleteFieldAsync_DeletesField()
        {
            var fieldId = Guid.NewGuid();
            _repo.DeleteAsync(fieldId).Returns(true);
            var service = new FormFieldService(_repo, _mapper);
            var result = await service.DeleteFieldAsync(fieldId);
            Assert.True(result);
        }

        [Fact]
        public async Task ReorderFieldsAsync_UpdatesOrder()
        {
            var versionId = Guid.NewGuid();
            var fieldIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var fields = new List<FormVersionField> {
                new FormVersionField { Id = fieldIds[0], Order = 2 },
                new FormVersionField { Id = fieldIds[1], Order = 1 }
            };
            _repo.GetFieldsByVersionIdAsync(versionId).Returns(fields);
            _repo.BulkUpdateAsync(Arg.Any<List<FormVersionField>>()).Returns(true);
            var service = new FormFieldService(_repo, _mapper);
            var result = await service.ReorderFieldsAsync(versionId, fieldIds);
            Assert.True(result);
        }

        [Fact]
        public async Task BulkCreateFieldsAsync_CreatesFields()
        {
            var versionId = Guid.NewGuid();
            var createDtos = new List<CreateFormFieldDto> { new CreateFormFieldDto { Name = "Bulk1" } };
            var entities = new List<FormVersionField> { new FormVersionField { Id = Guid.NewGuid(), Name = "Bulk1" } };
            var resultDtos = new List<FormFieldDto> { new FormFieldDto { Id = entities[0].Id, Name = "Bulk1" } };
            _mapper.Map<List<FormVersionField>>(createDtos).Returns(entities);
            _repo.BulkCreateAsync(entities).Returns(entities);
            _mapper.Map<IEnumerable<FormFieldDto>>(entities).Returns(resultDtos);
            var service = new FormFieldService(_repo, _mapper);
            var result = await service.BulkCreateFieldsAsync(versionId, createDtos);
            Assert.Single(result);
            Assert.Equal("Bulk1", result.First().Name);
        }

        [Fact]
        public async Task BulkUpdateFieldsAsync_UpdatesFields()
        {
            var versionId = Guid.NewGuid();
            var updateDtos = new List<UpdateFormFieldDto> { new UpdateFormFieldDto { Name = "BulkUpdate1" } };
            var entities = new List<FormVersionField> { new FormVersionField { Id = Guid.NewGuid(), Name = "BulkUpdate1" } };
            _mapper.Map<List<FormVersionField>>(updateDtos).Returns(entities);
            _repo.BulkUpdateAsync(entities).Returns(true);
            var service = new FormFieldService(_repo, _mapper);
            var result = await service.BulkUpdateFieldsAsync(versionId, updateDtos);
            Assert.True(result);
        }
    }
}
