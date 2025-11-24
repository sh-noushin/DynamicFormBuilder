using System;
using System.Threading.Tasks;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Services;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Repositories;
using NSubstitute;
using Xunit;

namespace FormBuilder.Tests.Services
{
    public class FormVersionServiceTests
    {
        private readonly IFormVersionRepository _versionRepo = Substitute.For<IFormVersionRepository>();
        private readonly IFormRepository _formRepo = Substitute.For<IFormRepository>();
        private readonly AutoMapper.IMapper _mapper = Substitute.For<AutoMapper.IMapper>();

        [Fact]
        public async Task GetVersionByIdAsync_ReturnsVersion()
        {
            var version = new FormBuilder.Models.Entities.FormVersion { Id = Guid.NewGuid(), Description = "desc" };
            var versionDto = new FormVersionDto { Id = version.Id, Description = version.Description };
            _versionRepo.GetVersionByIdAsync(version.Id).Returns(Task.FromResult<FormBuilder.Models.Entities.FormVersion?>(version));
            _mapper.Map<FormVersionDto>(version).Returns(versionDto);
            var service = new FormVersionService(_versionRepo, _formRepo, _mapper);
            var result = await service.GetVersionByIdAsync(version.Id);
            Assert.NotNull(result);
            Assert.Equal("desc", result.Description);
        }

        [Fact]
        public async Task GetVersionsByFormIdAsync_ReturnsVersions()
        {
            var formId = Guid.NewGuid();
            var versions = new List<FormBuilder.Models.Entities.FormVersion> { new FormBuilder.Models.Entities.FormVersion { Id = Guid.NewGuid(), Description = "v1" } };
            var versionDtos = new List<FormVersionDto> { new FormVersionDto { Id = versions[0].Id, Description = "v1" } };
            _versionRepo.GetVersionsByFormIdAsync(formId).Returns(versions);
            _mapper.Map<IEnumerable<FormVersionDto>>(versions).Returns(versionDtos);
            var service = new FormVersionService(_versionRepo, _formRepo, _mapper);
            var result = await service.GetVersionsByFormIdAsync(formId);
            Assert.Single(result);
            Assert.Equal("v1", result.First().Description);
        }

        [Fact]
        public async Task GetVersionAsync_ReturnsVersion()
        {
            var formId = Guid.NewGuid();
            var versionNumber = 1;
            var version = new FormBuilder.Models.Entities.FormVersion { Id = Guid.NewGuid(), Description = "desc", VersionNumber = versionNumber };
            var versionDto = new FormVersionDto { Id = version.Id, Description = version.Description, VersionNumber = versionNumber };
            _versionRepo.GetVersionAsync(formId, versionNumber).Returns(version);
            _mapper.Map<FormVersionDto>(version).Returns(versionDto);
            var service = new FormVersionService(_versionRepo, _formRepo, _mapper);
            var result = await service.GetVersionAsync(formId, versionNumber);
            Assert.NotNull(result);
            Assert.Equal("desc", result.Description);
            Assert.Equal(versionNumber, result.VersionNumber);
        }

        [Fact]
        public async Task GetCurrentVersionAsync_ReturnsCurrentVersion()
        {
            var formId = Guid.NewGuid();
            var version = new FormBuilder.Models.Entities.FormVersion { Id = Guid.NewGuid(), Description = "current", IsCurrentVersion = true };
            var versionDto = new FormVersionDto { Id = version.Id, Description = version.Description, IsCurrentVersion = true };
            _versionRepo.GetCurrentVersionAsync(formId).Returns(version);
            _mapper.Map<FormVersionDto>(version).Returns(versionDto);
            var service = new FormVersionService(_versionRepo, _formRepo, _mapper);
            var result = await service.GetCurrentVersionAsync(formId);
            Assert.NotNull(result);
            Assert.True(result.IsCurrentVersion);
        }

        [Fact]
        public async Task CreateVersionAsync_CreatesVersion()
        {
            var formId = Guid.NewGuid();
            var createDto = new CreateFormVersionDto { Description = "new version" };
            var form = new FormBuilder.Models.Entities.Form { Id = formId };
            var existingVersions = new List<FormBuilder.Models.Entities.FormVersion>();
            var entity = new FormBuilder.Models.Entities.FormVersion { Id = Guid.NewGuid(), Description = "new version", FormId = formId, VersionNumber = 1 };
            var resultDto = new FormVersionDto { Id = entity.Id, Description = entity.Description, VersionNumber = 1 };
            _formRepo.GetByIdAsync(formId).Returns(form);
            _versionRepo.GetVersionsByFormIdAsync(formId).Returns(existingVersions);
            _mapper.Map<FormBuilder.Models.Entities.FormVersion>(createDto).Returns(entity);
            _versionRepo.CreateAsync(entity).Returns(entity);
            _mapper.Map<FormVersionDto>(entity).Returns(resultDto);
            var service = new FormVersionService(_versionRepo, _formRepo, _mapper);
            var result = await service.CreateVersionAsync(formId, createDto);
            Assert.NotNull(result);
            Assert.Equal("new version", result.Description);
            Assert.Equal(1, result.VersionNumber);
        }

        [Fact]
        public async Task UpdateVersionAsync_UpdatesVersion()
        {
            var formId = Guid.NewGuid();
            var versionNumber = 2;
            var updateDto = new UpdateFormVersionDto { Description = "updated version" };
            var entity = new FormBuilder.Models.Entities.FormVersion { Id = Guid.NewGuid(), Description = "updated version", VersionNumber = versionNumber };
            var updatedEntity = new FormBuilder.Models.Entities.FormVersion { Id = entity.Id, Description = "updated version", VersionNumber = versionNumber };
            var resultDto = new FormVersionDto { Id = entity.Id, Description = "updated version", VersionNumber = versionNumber };
            _mapper.Map<FormBuilder.Models.Entities.FormVersion>(updateDto).Returns(entity);
            _versionRepo.UpdateAsync(formId, versionNumber, entity).Returns(updatedEntity);
            _mapper.Map<FormVersionDto>(updatedEntity).Returns(resultDto);
            var service = new FormVersionService(_versionRepo, _formRepo, _mapper);
            var result = await service.UpdateVersionAsync(formId, versionNumber, updateDto);
            Assert.NotNull(result);
            Assert.Equal("updated version", result.Description);
            Assert.Equal(versionNumber, result.VersionNumber);
        }

        [Fact]
        public async Task DeleteVersionAsync_DeletesVersion()
        {
            var formId = Guid.NewGuid();
            var versionNumber = 3;
            _versionRepo.DeleteAsync(formId, versionNumber).Returns(true);
            var service = new FormVersionService(_versionRepo, _formRepo, _mapper);
            var result = await service.DeleteVersionAsync(formId, versionNumber);
            Assert.True(result);
        }

        [Fact]
        public async Task PublishVersionAsync_PublishesVersion()
        {
            var formId = Guid.NewGuid();
            var versionNumber = 4;
            var version = new FormBuilder.Models.Entities.FormVersion { Id = Guid.NewGuid(), VersionNumber = versionNumber, IsPublished = false };
            var updatedVersion = new FormBuilder.Models.Entities.FormVersion { Id = version.Id, VersionNumber = versionNumber, IsPublished = true };
            _versionRepo.GetVersionAsync(formId, versionNumber).Returns(version);
            _versionRepo.UpdateAsync(formId, versionNumber, Arg.Any<FormBuilder.Models.Entities.FormVersion>()).Returns(updatedVersion);
            var service = new FormVersionService(_versionRepo, _formRepo, _mapper);
            var result = await service.PublishVersionAsync(formId, versionNumber);
            Assert.True(result);
        }

        [Fact]
        public async Task SetCurrentVersionAsync_SetsCurrentVersion()
        {
            var formId = Guid.NewGuid();
            var versionNumber = 5;
            var allVersions = new List<FormBuilder.Models.Entities.FormVersion> {
                new FormBuilder.Models.Entities.FormVersion { Id = Guid.NewGuid(), VersionNumber = 1, IsCurrentVersion = true },
                new FormBuilder.Models.Entities.FormVersion { Id = Guid.NewGuid(), VersionNumber = versionNumber, IsCurrentVersion = false }
            };
            var targetVersion = allVersions[1];
            _versionRepo.GetVersionsByFormIdAsync(formId).Returns(allVersions);
            _versionRepo.GetVersionAsync(formId, versionNumber).Returns(targetVersion);
            _versionRepo.UpdateAsync(formId, 1, Arg.Any<FormBuilder.Models.Entities.FormVersion>()).Returns(allVersions[0]);
            _versionRepo.UpdateAsync(formId, versionNumber, targetVersion).Returns(targetVersion);
            var service = new FormVersionService(_versionRepo, _formRepo, _mapper);
            var result = await service.SetCurrentVersionAsync(formId, versionNumber);
            Assert.True(result);
        }
    }
}
