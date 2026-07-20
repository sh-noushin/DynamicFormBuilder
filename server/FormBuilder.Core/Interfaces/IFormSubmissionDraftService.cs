using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IFormSubmissionDraftService
{
    // Persists or refreshes a draft against the form identified by slug.
    // When payload.ResumeToken is null a new draft (with a fresh token) is
    // created; otherwise the matching draft is updated in place. Returns the
    // draft's persistent state including ExpiresAt so the client can display
    // it. Throws FormNotFoundException if the slug does not resolve.
    Task<FormSubmissionDraftDto> SaveAsync(string slug, SaveDraftDto payload);

    // Fetches a draft by resume token, scoped to the form's slug so a token
    // leak on one form can't be used to enumerate drafts on another form.
    // Returns null when the token is unknown or the draft has expired.
    Task<FormSubmissionDraftDto?> GetAsync(string slug, Guid resumeToken);
}
