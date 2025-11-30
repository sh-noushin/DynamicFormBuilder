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
    public class FormSubmissionServiceTests
    {
        private readonly IFormSubmissionRepository _repo = Substitute.For<IFormSubmissionRepository>();
        private readonly AutoMapper.IMapper _mapper = Substitute.For<AutoMapper.IMapper>();

        [Fact]
        public async Task GetSubmissionByIdAsync_ReturnsSubmission()
        {
            var submission = new FormBuilder.Models.Entities.FormSubmission { Id = Guid.NewGuid(), SubmitterName = "User" };
            var submissionDto = new FormSubmissionDto { Id = submission.Id, SubmitterName = submission.SubmitterName };
            _repo.GetByIdAsync(submission.Id).Returns(Task.FromResult<FormBuilder.Models.Entities.FormSubmission?>(submission));
            _mapper.Map<FormSubmissionDto>(submission).Returns(submissionDto);
            var validator = Substitute.For<FormBuilder.Core.Interfaces.IFieldValidator>();
            var service = new FormSubmissionService(_repo, _mapper, validator);
            var result = await service.GetSubmissionByIdAsync(submission.Id);
            Assert.NotNull(result);
            Assert.Equal("User", result.SubmitterName);
        }

        [Fact]
        public async Task CreateSubmissionAsync_CreatesSubmission()
        {
            var createDto = new CreateFormSubmissionDto { SubmitterName = "User" };
            var entity = new FormBuilder.Models.Entities.FormSubmission { Id = Guid.NewGuid(), SubmitterName = "User" };
            var resultDto = new FormSubmissionDto { Id = entity.Id, SubmitterName = entity.SubmitterName };
            _mapper.Map<FormBuilder.Models.Entities.FormSubmission>(createDto).Returns(entity);
            _repo.CreateAsync(entity).Returns(entity);
            _mapper.Map<FormSubmissionDto>(entity).Returns(resultDto);
            var validator = Substitute.For<FormBuilder.Core.Interfaces.IFieldValidator>();
            var service = new FormSubmissionService(_repo, _mapper, validator);
            var result = await service.CreateSubmissionAsync(createDto);
            Assert.NotNull(result);
            Assert.Equal("User", result.SubmitterName);
        }

        [Fact]
        public async Task GetSubmissionsByFormVersionIdAsync_ReturnsSubmissions()
        {
            var versionId = Guid.NewGuid();
            var submissions = new List<FormBuilder.Models.Entities.FormSubmission> { new FormBuilder.Models.Entities.FormSubmission { Id = Guid.NewGuid() } };
            var submissionDtos = new List<FormSubmissionDto> { new FormSubmissionDto { Id = submissions[0].Id } };
            _repo.GetByFormVersionIdAsync(versionId).Returns(submissions);
            _mapper.Map<IEnumerable<FormSubmissionDto>>(submissions).Returns(submissionDtos);
            var validator = Substitute.For<FormBuilder.Core.Interfaces.IFieldValidator>();
            var service = new FormSubmissionService(_repo, _mapper, validator);
            var result = await service.GetSubmissionsByFormVersionIdAsync(versionId);
            Assert.Single(result);
            Assert.Equal(submissions[0].Id, result.First().Id);
        }

        [Fact]
        public async Task GetSubmissionsByFormIdAsync_ReturnsSubmissions()
        {
            var formId = Guid.NewGuid();
            var submissions = new List<FormBuilder.Models.Entities.FormSubmission> { new FormBuilder.Models.Entities.FormSubmission { Id = Guid.NewGuid() } };
            var submissionDtos = new List<FormSubmissionDto> { new FormSubmissionDto { Id = submissions[0].Id } };
            _repo.GetByFormIdAsync(formId).Returns(submissions);
            _mapper.Map<IEnumerable<FormSubmissionDto>>(submissions).Returns(submissionDtos);
            var validator = Substitute.For<FormBuilder.Core.Interfaces.IFieldValidator>();
            var service = new FormSubmissionService(_repo, _mapper, validator);
            var result = await service.GetSubmissionsByFormIdAsync(formId);
            Assert.Single(result);
            Assert.Equal(submissions[0].Id, result.First().Id);
        }

        [Fact]
        public async Task GetSubmissionCountByFormVersionIdAsync_ReturnsCount()
        {
            var versionId = Guid.NewGuid();
            _repo.GetSubmissionCountByFormVersionIdAsync(versionId).Returns(5);
            var validator = Substitute.For<FormBuilder.Core.Interfaces.IFieldValidator>();
            var service = new FormSubmissionService(_repo, _mapper, validator);
            var result = await service.GetSubmissionCountByFormVersionIdAsync(versionId);
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task UpdateSubmissionAsync_UpdatesSubmission()
        {
            var submissionId = Guid.NewGuid();
            var updateDto = new UpdateFormSubmissionDto { SubmitterName = "Updated" };
            var updatedEntity = new FormBuilder.Models.Entities.FormSubmission { Id = submissionId, SubmitterName = "Updated" };
            var resultDto = new FormSubmissionDto { Id = submissionId, SubmitterName = "Updated" };
            _repo.UpdateAsync(submissionId, updateDto.SubmitterName, updateDto.SubmitterEmail, updateDto.FieldValues).Returns(updatedEntity);
            _mapper.Map<FormSubmissionDto>(updatedEntity).Returns(resultDto);
            var validator = Substitute.For<FormBuilder.Core.Interfaces.IFieldValidator>();
            var service = new FormSubmissionService(_repo, _mapper, validator);
            var result = await service.UpdateSubmissionAsync(submissionId, updateDto);
            Assert.NotNull(result);
            Assert.Equal("Updated", result.SubmitterName);
        }

        [Fact]
        public async Task DeleteSubmissionAsync_DeletesSubmission()
        {
            var submissionId = Guid.NewGuid();
            _repo.DeleteAsync(submissionId).Returns(true);
            var validator = Substitute.For<FormBuilder.Core.Interfaces.IFieldValidator>();
            var service = new FormSubmissionService(_repo, _mapper, validator);
            var result = await service.DeleteSubmissionAsync(submissionId);
            Assert.True(result);
        }
    }
}
