using AutoMapper;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;

namespace FormBuilder.Core.Services;

public class PublicFormService : IPublicFormService
{
    private readonly IFormRepository _formRepository;
    private readonly IFormSubmissionRepository _submissionRepository;
    private readonly IMapper _mapper;
    private readonly IFieldValidator _validator;

    public PublicFormService(
        IFormRepository formRepository,
        IFormSubmissionRepository submissionRepository,
        IMapper mapper,
        IFieldValidator validator)
    {
        _formRepository = formRepository;
        _submissionRepository = submissionRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PublicFormDto> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be null or empty.", nameof(slug));

        var (form, version) = await ResolvePublishedCurrentVersionAsync(slug);

        return new PublicFormDto
        {
            Slug = form.Slug,
            Name = form.Name,
            Description = form.Description,
            FormVersionId = version.Id,
            VersionNumber = version.VersionNumber,
            Fields = _mapper.Map<List<FormFieldDto>>(version.Fields)
        };
    }

    public async Task<FormSubmissionDto> SubmitAsync(string slug, PublicFormSubmissionDto submission)
    {
        if (submission == null)
            throw new ArgumentNullException(nameof(submission));

        var (_, version) = await ResolvePublishedCurrentVersionAsync(slug);

        var validationErrors = await _validator.ValidateAsync(version.Id, submission.FieldValues);
        if (validationErrors != null && validationErrors.Count > 0)
        {
            throw new FormSubmissionValidationException(
                "Submission contains validation errors.",
                validationErrors.ToDictionary(kv => kv.Key, kv => (IEnumerable<string>)kv.Value));
        }

        var entity = new FormSubmission
        {
            FormVersionId = version.Id,
            SubmittedAt = DateTime.UtcNow,
            SubmitterName = submission.SubmitterName,
            SubmitterEmail = submission.SubmitterEmail,
            Values = submission.FieldValues
                .Select(kv => new FormSubmissionValue
                {
                    FieldName = kv.Key,
                    FieldValue = kv.Value
                })
                .ToList()
        };

        var created = await _submissionRepository.CreateAsync(entity);
        return _mapper.Map<FormSubmissionDto>(created);
    }

    // A missing form, an inactive form, or a form without a published current
    // version all produce the same FormNotFoundException so the endpoint does
    // not leak the difference to an anonymous caller.
    private async Task<(Form Form, FormVersion Version)> ResolvePublishedCurrentVersionAsync(string slug)
    {
        var form = await _formRepository.GetBySlugAsync(slug)
            ?? throw new FormNotFoundException($"No published form exists at '{slug}'.");

        if (!form.IsActive)
            throw new FormNotFoundException($"No published form exists at '{slug}'.");

        var currentVersion = form.Versions.FirstOrDefault(v => v.IsCurrentVersion && v.IsPublished);
        if (currentVersion == null)
            throw new FormNotFoundException($"No published form exists at '{slug}'.");

        return (form, currentVersion);
    }
}
