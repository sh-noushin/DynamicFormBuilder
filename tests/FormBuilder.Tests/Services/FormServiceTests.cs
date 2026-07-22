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
    public class FormServiceTests
    {
        private readonly IFormRepository _repo = Substitute.For<IFormRepository>();
        private readonly AutoMapper.IMapper _mapper = Substitute.For<AutoMapper.IMapper>();
        private readonly ICurrentUserService _currentUser = MakeCurrentUser();
        private readonly IOrganizationRepository _orgs = Substitute.For<IOrganizationRepository>();

        private static ICurrentUserService MakeCurrentUser()
        {
            var svc = Substitute.For<ICurrentUserService>();
            svc.GetOrganizationId().Returns(Guid.NewGuid());
            svc.GetOrganizationIdOrNull().Returns((Guid?)null);
            return svc;
        }

        [Fact]
        public async Task GetFormByIdAsync_ReturnsForm()
        {
            var form = new FormBuilder.Models.Entities.Form { Id = Guid.NewGuid(), Name = "Test" };
            var formDto = new FormBuilder.Core.DTOs.FormDto { Id = form.Id, Name = form.Name };
            _repo.GetByIdAsync(form.Id).Returns(Task.FromResult<FormBuilder.Models.Entities.Form?>(form));
            _mapper.Map<FormBuilder.Core.DTOs.FormDto>(form).Returns(formDto);
            var service = new FormService(_repo, _mapper, _currentUser, _orgs);
            var result = await service.GetFormByIdAsync(form.Id);
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task GetAllFormsAsync_ReturnsForms()
        {
            var forms = new List<FormBuilder.Models.Entities.Form> { new FormBuilder.Models.Entities.Form { Id = Guid.NewGuid(), Name = "Form1" } };
            var formDtos = new List<FormBuilder.Core.DTOs.FormDto> { new FormBuilder.Core.DTOs.FormDto { Id = forms[0].Id, Name = forms[0].Name } };
            _repo.GetAllAsync().Returns(forms);
            _mapper.Map<IEnumerable<FormBuilder.Core.DTOs.FormDto>>(forms).Returns(formDtos);
            var service = new FormService(_repo, _mapper, _currentUser, _orgs);
            var result = await service.GetAllFormsAsync();
            Assert.Single(result);
            Assert.Equal("Form1", result.First().Name);
        }

        [Fact]
        public async Task CreateFormAsync_CreatesForm()
        {
            var createDto = new FormBuilder.Core.DTOs.CreateFormDto { Name = "NewForm" };
            var entity = new FormBuilder.Models.Entities.Form { Id = Guid.NewGuid(), Name = "NewForm" };
            var resultDto = new FormBuilder.Core.DTOs.FormDto { Id = entity.Id, Name = entity.Name };
            _mapper.Map<FormBuilder.Models.Entities.Form>(createDto).Returns(entity);
            _repo.CreateAsync(entity).Returns(entity);
            _mapper.Map<FormBuilder.Core.DTOs.FormDto>(entity).Returns(resultDto);
            var service = new FormService(_repo, _mapper, _currentUser, _orgs);
            var result = await service.CreateFormAsync(createDto);
            Assert.NotNull(result);
            Assert.Equal("NewForm", result.Name);
        }

        [Fact]
        public async Task UpdateFormAsync_UpdatesForm()
        {
            var formId = Guid.NewGuid();
            var updateDto = new FormBuilder.Core.DTOs.UpdateFormDto { Name = "UpdatedForm" };
            var entity = new FormBuilder.Models.Entities.Form { Id = formId, Name = "UpdatedForm" };
            var updatedEntity = new FormBuilder.Models.Entities.Form { Id = formId, Name = "UpdatedForm" };
            var resultDto = new FormBuilder.Core.DTOs.FormDto { Id = formId, Name = "UpdatedForm" };
            _mapper.Map<FormBuilder.Models.Entities.Form>(updateDto).Returns(entity);
            _repo.UpdateAsync(formId, entity).Returns(updatedEntity);
            _mapper.Map<FormBuilder.Core.DTOs.FormDto>(updatedEntity).Returns(resultDto);
            var service = new FormService(_repo, _mapper, _currentUser, _orgs);
            var result = await service.UpdateFormAsync(formId, updateDto);
            Assert.NotNull(result);
            Assert.Equal("UpdatedForm", result.Name);
        }

        [Fact]
        public async Task DeleteFormAsync_DeletesForm()
        {
            var formId = Guid.NewGuid();
            _repo.DeleteAsync(formId).Returns(true);
            var service = new FormService(_repo, _mapper, _currentUser, _orgs);
            var result = await service.DeleteFormAsync(formId);
            Assert.True(result);
        }

        [Fact]
        public async Task ActivateFormAsync_ActivatesForm()
        {
            var formId = Guid.NewGuid();
            var form = new FormBuilder.Models.Entities.Form { Id = formId, Name = "ActiveForm", IsActive = false };
            var updatedForm = new FormBuilder.Models.Entities.Form { Id = formId, Name = "ActiveForm", IsActive = true };
            _repo.GetByIdAsync(formId).Returns(form);
            _repo.UpdateAsync(formId, Arg.Any<FormBuilder.Models.Entities.Form>()).Returns(updatedForm);
            var service = new FormService(_repo, _mapper, _currentUser, _orgs);
            var result = await service.ActivateFormAsync(formId);
            Assert.True(result);
        }

        [Fact]
        public async Task DeactivateFormAsync_DeactivatesForm()
        {
            var formId = Guid.NewGuid();
            var form = new FormBuilder.Models.Entities.Form { Id = formId, Name = "InactiveForm", IsActive = true };
            var updatedForm = new FormBuilder.Models.Entities.Form { Id = formId, Name = "InactiveForm", IsActive = false };
            _repo.GetByIdAsync(formId).Returns(form);
            _repo.UpdateAsync(formId, Arg.Any<FormBuilder.Models.Entities.Form>()).Returns(updatedForm);
            var service = new FormService(_repo, _mapper, _currentUser, _orgs);
            var result = await service.DeactivateFormAsync(formId);
            Assert.True(result);
        }
    }

}