using AutoMapper;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;

namespace FormBuilder.Core.Services;

public class FormSubmissionService : IFormSubmissionService
{
    private readonly IFormSubmissionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IFieldValidator _validator;

    public FormSubmissionService(IFormSubmissionRepository repository, IMapper mapper, IFieldValidator validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<FormSubmissionDto> CreateSubmissionAsync(CreateFormSubmissionDto submissionDto)
    {
        if (submissionDto == null)
            throw new ArgumentNullException(nameof(submissionDto), "Submission cannot be null.");

        try
        {
            var validationErrors = await _validator.ValidateAsync(submissionDto.FormVersionId, submissionDto.FieldValues);
            if (validationErrors != null && validationErrors.Count > 0)
            {
                throw new FormBuilder.Models.Exceptions.FormSubmissionValidationException("Submission contains validation errors.", validationErrors.ToDictionary(kv => kv.Key, kv => (IEnumerable<string>)kv.Value));
            }

            var entity = _mapper.Map<FormBuilder.Models.Entities.FormSubmission>(submissionDto);
            entity.SubmittedAt = DateTime.UtcNow;

            if ((entity.Values == null || entity.Values.Count == 0) && submissionDto.FieldValues != null && submissionDto.FieldValues.Count > 0)
            {
                entity.Values = submissionDto.FieldValues
                    .Select(kv => new FormBuilder.Models.Entities.FormSubmissionValue
                    {
                        FieldName = kv.Key,
                        FieldValue = kv.Value
                    })
                    .ToList();
            }

            entity.SubmitterName = entity.SubmitterName ?? submissionDto.SubmitterName;
            entity.SubmitterEmail = entity.SubmitterEmail ?? submissionDto.SubmitterEmail;
            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<FormSubmissionDto>(created);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the submission.", ex);
        }
    }

    public async Task<FormSubmissionDto> GetSubmissionByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty.", nameof(id));

        try
        {
            var submission = await _repository.GetByIdAsync(id);
            if (submission == null)
                throw new FormSubmissionNotFoundException(id);
            return _mapper.Map<FormSubmissionDto>(submission);
        }
        catch (FormSubmissionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving submission with ID {id}.", ex);
        }
    }

    public async Task<IEnumerable<FormSubmissionDto>> GetSubmissionsByFormVersionIdAsync(Guid formVersionId)
    {
        if (formVersionId == Guid.Empty)
            throw new ArgumentException("Form version ID cannot be empty.", nameof(formVersionId));

        try
        {
            var submissions = await _repository.GetByFormVersionIdAsync(formVersionId);
            return _mapper.Map<IEnumerable<FormSubmissionDto>>(submissions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving submissions for form version ID {formVersionId}.", ex);
        }
    }

    public async Task<IEnumerable<FormSubmissionDto>> GetSubmissionsByFormIdAsync(Guid formId)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));

        try
        {
            var submissions = await _repository.GetByFormIdAsync(formId);
            return _mapper.Map<IEnumerable<FormSubmissionDto>>(submissions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving submissions for form ID {formId}.", ex);
        }
    }

    public async Task<int> GetSubmissionCountByFormVersionIdAsync(Guid formVersionId)
    {
        if (formVersionId == Guid.Empty)
            throw new ArgumentException("Form version ID cannot be empty.", nameof(formVersionId));

        try
        {
            return await _repository.GetSubmissionCountByFormVersionIdAsync(formVersionId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving submission count for form version ID {formVersionId}.", ex);
        }
    }

    public async Task<FormSubmissionDto> UpdateSubmissionAsync(Guid id, UpdateFormSubmissionDto updateDto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty.", nameof(id));
        if (updateDto == null)
            throw new ArgumentNullException(nameof(updateDto), "Update DTO cannot be null.");

        try
        {
            var updated = await _repository.UpdateAsync(id, updateDto.SubmitterName, updateDto.SubmitterEmail, updateDto.FieldValues);
            return _mapper.Map<FormSubmissionDto>(updated);
        }
        catch (FormSubmissionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while updating submission with ID {id}.", ex);
        }
    }

    public async Task<bool> DeleteSubmissionAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty.", nameof(id));

        try
        {
            var result = await _repository.DeleteAsync(id);
            if (!result)
                throw new FormSubmissionNotFoundException(id);
            return result;
        }
        catch (FormSubmissionNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while deleting submission with ID {id}.", ex);
        }
    }
}
