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
    private readonly ISubmissionNotifier _notifier;
    private readonly IFormRepository _formRepository;
    private readonly IFormVersionRepository _versionRepository;

    public FormSubmissionService(
        IFormSubmissionRepository repository,
        IMapper mapper,
        IFieldValidator validator,
        ISubmissionNotifier notifier,
        IFormRepository formRepository,
        IFormVersionRepository versionRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _notifier = notifier;
        _formRepository = formRepository;
        _versionRepository = versionRepository;
    }

    public async Task<FormSubmissionDto> CreateSubmissionAsync(CreateFormSubmissionDto submissionDto)
    {
        if (submissionDto == null)
            throw new ArgumentNullException(nameof(submissionDto), "Submission cannot be null.");

        var validationErrors = await _validator.ValidateAsync(submissionDto.FormVersionId, submissionDto.FieldValues);
        if (validationErrors != null && validationErrors.Count > 0)
        {
            throw new FormSubmissionValidationException(
                "Submission contains validation errors.",
                validationErrors.ToDictionary(kv => kv.Key, kv => (IEnumerable<string>)kv.Value));
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

        entity.SubmitterName ??= submissionDto.SubmitterName;
        entity.SubmitterEmail ??= submissionDto.SubmitterEmail;
        var created = await _repository.CreateAsync(entity);
        var dto = _mapper.Map<FormSubmissionDto>(created);
        await NotifyAsync(created.FormVersionId, dto);
        return dto;
    }

    private async Task NotifyAsync(Guid formVersionId, FormSubmissionDto dto)
    {
        var version = await _versionRepository.GetVersionByIdAsync(formVersionId);
        if (version == null) return;
        var form = await _formRepository.GetByIdAsync(version.FormId);
        var name = form?.Name ?? "(unknown form)";
        await _notifier.NotifyAsync(name, dto);
    }

    public async Task<FormSubmissionDto> GetSubmissionByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty.", nameof(id));

        var submission = await _repository.GetByIdAsync(id);
        if (submission == null)
            throw new FormSubmissionNotFoundException(id);
        return _mapper.Map<FormSubmissionDto>(submission);
    }

    public async Task<IEnumerable<FormSubmissionDto>> GetSubmissionsByFormVersionIdAsync(Guid formVersionId)
    {
        if (formVersionId == Guid.Empty)
            throw new ArgumentException("Form version ID cannot be empty.", nameof(formVersionId));

        var submissions = await _repository.GetByFormVersionIdAsync(formVersionId);
        return _mapper.Map<IEnumerable<FormSubmissionDto>>(submissions);
    }

    public async Task<IEnumerable<FormSubmissionDto>> GetSubmissionsByFormIdAsync(Guid formId)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));

        var submissions = await _repository.GetByFormIdAsync(formId);
        return _mapper.Map<IEnumerable<FormSubmissionDto>>(submissions);
    }

    public async Task<int> GetSubmissionCountByFormVersionIdAsync(Guid formVersionId)
    {
        if (formVersionId == Guid.Empty)
            throw new ArgumentException("Form version ID cannot be empty.", nameof(formVersionId));

        return await _repository.GetSubmissionCountByFormVersionIdAsync(formVersionId);
    }

    public async Task<FormSubmissionDto> UpdateSubmissionAsync(Guid id, UpdateFormSubmissionDto updateDto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty.", nameof(id));
        if (updateDto == null)
            throw new ArgumentNullException(nameof(updateDto), "Update DTO cannot be null.");

        var source = new FormBuilder.Models.Entities.FormSubmission
        {
            SubmitterName = updateDto.SubmitterName,
            SubmitterEmail = updateDto.SubmitterEmail,
            Values = updateDto.FieldValues
                .Select(kv => new FormBuilder.Models.Entities.FormSubmissionValue
                {
                    FieldName = kv.Key,
                    FieldValue = kv.Value
                })
                .ToList()
        };

        var updated = await _repository.UpdateAsync(id, source);
        if (updated == null)
            throw new FormSubmissionNotFoundException(id);
        return _mapper.Map<FormSubmissionDto>(updated);
    }

    public async Task<bool> DeleteSubmissionAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty.", nameof(id));

        var result = await _repository.DeleteAsync(id);
        if (!result)
            throw new FormSubmissionNotFoundException(id);
        return result;
    }

    public async Task<FormSubmissionDto> UpdateAdminNotesAsync(Guid id, string? adminNotes)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty.", nameof(id));

        var updated = await _repository.UpdateAdminNotesAsync(id, adminNotes);
        if (updated == null)
            throw new FormSubmissionNotFoundException(id);
        return _mapper.Map<FormSubmissionDto>(updated);
    }

    // Normalizes user-provided tag input before persistence: trim + lower +
    // dedupe + drop empties. Individual labels are capped at 40 chars so a
    // rogue payload can't saturate the 500-char Tags column, and the total
    // count is capped at 20 tags per submission.
    private const int MaxTagLength = 40;
    private const int MaxTagCount = 20;

    public async Task<FormSubmissionDto> UpdateTagsAsync(Guid id, IEnumerable<string> tags)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Submission ID cannot be empty.", nameof(id));
        if (tags == null)
            throw new ArgumentNullException(nameof(tags));

        var normalized = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Select(t => t.Length > MaxTagLength ? t.Substring(0, MaxTagLength) : t)
            .Distinct()
            .Take(MaxTagCount)
            .ToList();

        var csv = normalized.Count == 0 ? null : string.Join(",", normalized);

        var updated = await _repository.UpdateTagsAsync(id, csv);
        if (updated == null)
            throw new FormSubmissionNotFoundException(id);
        return _mapper.Map<FormSubmissionDto>(updated);
    }

    public async Task<int> BulkDeleteSubmissionsAsync(Guid formId, IReadOnlyList<Guid> ids)
    {
        if (formId == Guid.Empty)
            throw new ArgumentException("Form ID cannot be empty.", nameof(formId));
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        // Drop empty and duplicate ids up front so the repository doesn't
        // have to defend against them.
        var normalized = ids.Where(id => id != Guid.Empty).Distinct().ToList();
        if (normalized.Count == 0) return 0;

        return await _repository.DeleteManyByFormAsync(formId, normalized);
    }
}
