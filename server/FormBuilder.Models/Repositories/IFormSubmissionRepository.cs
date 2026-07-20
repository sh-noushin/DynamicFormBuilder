using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IFormSubmissionRepository
{
    Task<FormSubmission> CreateAsync(FormSubmission submission);
    Task<FormSubmission?> GetByIdAsync(Guid id);
    Task<IEnumerable<FormSubmission>> GetByFormVersionIdAsync(Guid formVersionId);
    Task<IEnumerable<FormSubmission>> GetByFormIdAsync(Guid formId);
    Task<int> GetSubmissionCountByFormVersionIdAsync(Guid formVersionId);
    Task<int> GetSubmissionCountByFormIdAsync(Guid formId);
    Task<bool> HasSubmissionFromEmailAsync(Guid formId, string email);
    // Returns only the SubmittedAt timestamps for the form since 'since',
    // ordered ascending. Skips the payload/values so analytics queries stay
    // cheap even on high-volume forms.
    Task<IReadOnlyList<DateTime>> GetSubmittedAtByFormIdSinceAsync(Guid formId, DateTime since);
    Task<FormSubmission?> UpdateAsync(Guid id, FormSubmission source);
    // Notes-only update path so the caller doesn't have to round-trip the
    // full submission (SubmitterName / Email / all field values) just to
    // change a private admin annotation.
    Task<FormSubmission?> UpdateAdminNotesAsync(Guid id, string? adminNotes);
    // Tags-only update: writes a normalized CSV string to the Tags column
    // without touching notes, submitter info, or field values.
    Task<FormSubmission?> UpdateTagsAsync(Guid id, string? tagsCsv);
    Task<bool> DeleteAsync(Guid id);
    // Deletes every submission whose id is in ids AND whose parent form has
    // Id == formId. The formId scoping is a defense-in-depth guard so a
    // request for form A cannot delete submissions belonging to form B.
    // Returns the number of rows actually removed.
    Task<int> DeleteManyByFormAsync(Guid formId, IReadOnlyList<Guid> ids);
}
