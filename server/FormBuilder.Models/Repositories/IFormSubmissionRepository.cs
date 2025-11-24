using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IFormSubmissionRepository
{
    Task<FormSubmission> CreateAsync(FormSubmission submission);
    Task<FormSubmission?> GetByIdAsync(Guid id);
    Task<IEnumerable<FormSubmission>> GetByFormVersionIdAsync(Guid formVersionId);
    Task<IEnumerable<FormSubmission>> GetByFormIdAsync(Guid formId);
    Task<int> GetSubmissionCountByFormVersionIdAsync(Guid formVersionId);
    Task<FormSubmission> UpdateAsync(Guid id, string? submitterName, string? submitterEmail, Dictionary<string, string?> fieldValues);
    Task<bool> DeleteAsync(Guid id);
}
