using System.Text.Json;
using FormBuilder.Core.DTOs;
using FormBuilder.Core.Interfaces;
using FormBuilder.Models.Entities;
using FormBuilder.Models.Exceptions;
using FormBuilder.Models.Repositories;

namespace FormBuilder.Core.Services;

public class FormSubmissionDraftService : IFormSubmissionDraftService
{
    // Drafts age out after 30 days; every save refreshes the countdown. The
    // background here is that drafts have zero cost to the visitor but do
    // accumulate rows server-side, and stale ones are almost never resumed.
    private static readonly TimeSpan DraftLifetime = TimeSpan.FromDays(30);

    private readonly IFormRepository _formRepository;
    private readonly IFormSubmissionDraftRepository _draftRepository;

    public FormSubmissionDraftService(
        IFormRepository formRepository,
        IFormSubmissionDraftRepository draftRepository)
    {
        _formRepository = formRepository;
        _draftRepository = draftRepository;
    }

    public async Task<FormSubmissionDraftDto> SaveAsync(string slug, SaveDraftDto payload)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be null or empty.", nameof(slug));
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        var form = await _formRepository.GetBySlugAsync(slug)
            ?? throw new FormNotFoundException($"No form exists at '{slug}'.");
        if (!form.IsActive)
            throw new FormNotFoundException($"No form exists at '{slug}'.");

        var now = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(payload.FieldValues ?? new Dictionary<string, string?>());

        // Update-in-place when the client sends an existing token; otherwise
        // mint a brand-new draft. Update fails soft (falls through to create)
        // if the token no longer exists - a resumed link that has been pruned
        // should not throw for the visitor.
        if (payload.ResumeToken is Guid token && token != Guid.Empty)
        {
            var updated = await _draftRepository.UpdateAsync(new FormSubmissionDraft
            {
                ResumeToken = token,
                SubmitterName = payload.SubmitterName,
                SubmitterEmail = payload.SubmitterEmail,
                FieldValuesJson = json,
                UpdatedAt = now,
                ExpiresAt = now + DraftLifetime,
            });
            if (updated != null) return ToDto(updated);
        }

        var created = await _draftRepository.CreateAsync(new FormSubmissionDraft
        {
            FormId = form.Id,
            ResumeToken = Guid.NewGuid(),
            SubmitterName = payload.SubmitterName,
            SubmitterEmail = payload.SubmitterEmail,
            FieldValuesJson = json,
            CreatedAt = now,
            UpdatedAt = now,
            ExpiresAt = now + DraftLifetime,
        });
        return ToDto(created);
    }

    public async Task<FormSubmissionDraftDto?> GetAsync(string slug, Guid resumeToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be null or empty.", nameof(slug));

        var form = await _formRepository.GetBySlugAsync(slug);
        if (form == null || !form.IsActive) return null;

        var draft = await _draftRepository.GetByTokenAsync(resumeToken);
        if (draft == null) return null;
        // Scope check so a leaked token on form A can't fetch a draft on form
        // B. Also treat expired drafts as if they never existed.
        if (draft.FormId != form.Id) return null;
        if (draft.ExpiresAt <= DateTime.UtcNow) return null;

        return ToDto(draft);
    }

    private static FormSubmissionDraftDto ToDto(FormSubmissionDraft draft)
    {
        var values = string.IsNullOrWhiteSpace(draft.FieldValuesJson)
            ? new Dictionary<string, string?>()
            : (JsonSerializer.Deserialize<Dictionary<string, string?>>(draft.FieldValuesJson)
               ?? new Dictionary<string, string?>());
        return new FormSubmissionDraftDto
        {
            ResumeToken = draft.ResumeToken,
            ExpiresAt = draft.ExpiresAt,
            SubmitterName = draft.SubmitterName,
            SubmitterEmail = draft.SubmitterEmail,
            FieldValues = values,
        };
    }
}
