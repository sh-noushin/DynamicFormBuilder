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
    private readonly ISubmissionNotifier _notifier;
    private readonly IWebhookSender _webhookSender;

    public PublicFormService(
        IFormRepository formRepository,
        IFormSubmissionRepository submissionRepository,
        IMapper mapper,
        IFieldValidator validator,
        ISubmissionNotifier notifier,
        IWebhookSender webhookSender)
    {
        _formRepository = formRepository;
        _submissionRepository = submissionRepository;
        _mapper = mapper;
        _validator = validator;
        _notifier = notifier;
        _webhookSender = webhookSender;
    }

    public async Task<PublicFormDto> GetBySlugAsync(string slug, string? accessPassword)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be null or empty.", nameof(slug));

        var (form, version) = await ResolvePublishedCurrentVersionAsync(slug);

        // Closed check runs before the password gate so a visitor with the
        // correct password still can't submit against a closed form, and so
        // that the closed state is visible without needing the password.
        var closedReason = await GetClosedReasonAsync(form);
        if (closedReason != null)
        {
            return new PublicFormDto
            {
                Slug = form.Slug,
                Name = form.Name,
                Description = form.Description,
                BrandColor = form.BrandColor,
                IsClosed = true,
                ClosedReason = closedReason
            };
        }

        // If the form is password-protected and the caller has not supplied the
        // matching password, return a stub response that reveals only the name
        // and description so the client can render its unlock gate. Fields are
        // NOT returned.
        if (!string.IsNullOrEmpty(form.AccessPassword) && !PasswordMatches(form.AccessPassword, accessPassword))
        {
            return new PublicFormDto
            {
                Slug = form.Slug,
                Name = form.Name,
                Description = form.Description,
                BrandColor = form.BrandColor,
                RequiresPassword = true
            };
        }

        return new PublicFormDto
        {
            Slug = form.Slug,
            Name = form.Name,
            Description = form.Description,
            BrandColor = form.BrandColor,
            ThankYouMessage = form.ThankYouMessage,
            RedirectUrl = form.RedirectUrl,
            FormVersionId = version.Id,
            VersionNumber = version.VersionNumber,
            Fields = _mapper.Map<List<FormFieldDto>>(version.Fields)
        };
    }

    public async Task<FormSubmissionDto> SubmitAsync(string slug, PublicFormSubmissionDto submission, string? accessPassword)
    {
        if (submission == null)
            throw new ArgumentNullException(nameof(submission));

        var (form, version) = await ResolvePublishedCurrentVersionAsync(slug);

        // Same ordering as GetBySlugAsync: closed check first, then password.
        var closedReason = await GetClosedReasonAsync(form);
        if (closedReason != null)
        {
            throw new FormClosedException(closedReason);
        }

        // Enforce the password gate on submit too - the client sends the same
        // header on both GET and POST.
        if (!string.IsNullOrEmpty(form.AccessPassword) && !PasswordMatches(form.AccessPassword, accessPassword))
        {
            throw new InvalidCredentialsException("Incorrect form password.");
        }

        var validationErrors = await _validator.ValidateAsync(version.Id, submission.FieldValues);
        if (validationErrors != null && validationErrors.Count > 0)
        {
            throw new FormSubmissionValidationException(
                "Submission contains validation errors.",
                validationErrors.ToDictionary(kv => kv.Key, kv => (IEnumerable<string>)kv.Value));
        }

        // Duplicate check runs after field validation so a malformed submission
        // reports its field errors first; anonymous submissions (no email)
        // skip the check because there is nothing to dedupe against.
        if (form.OneResponsePerEmail && !string.IsNullOrWhiteSpace(submission.SubmitterEmail))
        {
            if (await _submissionRepository.HasSubmissionFromEmailAsync(form.Id, submission.SubmitterEmail))
            {
                throw new DuplicateSubmissionException();
            }
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
        var dto = _mapper.Map<FormSubmissionDto>(created);
        await _notifier.NotifyAsync(form.Name, dto);
        await _webhookSender.SendAsync(form, dto);
        return dto;
    }

    // Returns a user-facing reason string when the form is closed, or null when
    // it is still accepting submissions. Checks ClosesAt first (cheap), then
    // MaxSubmissions (one extra count query).
    private async Task<string?> GetClosedReasonAsync(Form form)
    {
        if (form.ClosesAt.HasValue && DateTime.UtcNow >= form.ClosesAt.Value)
        {
            return "This form has closed and is no longer accepting responses.";
        }
        if (form.MaxSubmissions.HasValue)
        {
            var count = await _submissionRepository.GetSubmissionCountByFormIdAsync(form.Id);
            if (count >= form.MaxSubmissions.Value)
            {
                return "This form has reached its response limit.";
            }
        }
        return null;
    }

    // Constant-time-ish comparison. AccessPassword is stored plaintext (form-
    // level shared secret, not user auth), but we still avoid an early-exit
    // string compare so basic timing side channels do not leak the length.
    private static bool PasswordMatches(string expected, string? supplied)
    {
        if (supplied == null) return false;
        if (expected.Length != supplied.Length) return false;
        var diff = 0;
        for (var i = 0; i < expected.Length; i++)
        {
            diff |= expected[i] ^ supplied[i];
        }
        return diff == 0;
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
