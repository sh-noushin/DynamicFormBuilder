using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IFormSubmissionDraftRepository
{
    Task<FormSubmissionDraft?> GetByTokenAsync(Guid resumeToken);
    Task<FormSubmissionDraft> CreateAsync(FormSubmissionDraft draft);
    Task<FormSubmissionDraft?> UpdateAsync(FormSubmissionDraft draft);
}
