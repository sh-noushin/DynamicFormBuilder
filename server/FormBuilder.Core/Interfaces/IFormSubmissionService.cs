using FormBuilder.Core.DTOs;

namespace FormBuilder.Core.Interfaces;

public interface IFormSubmissionService
{
    Task<FormSubmissionDto> CreateSubmissionAsync(CreateFormSubmissionDto submissionDto);
    Task<FormSubmissionDto> GetSubmissionByIdAsync(Guid id);
    Task<IEnumerable<FormSubmissionDto>> GetSubmissionsByFormVersionIdAsync(Guid formVersionId);
    Task<IEnumerable<FormSubmissionDto>> GetSubmissionsByFormIdAsync(Guid formId);
    Task<int> GetSubmissionCountByFormVersionIdAsync(Guid formVersionId);
    Task<FormSubmissionDto> UpdateSubmissionAsync(Guid id, UpdateFormSubmissionDto updateDto);
    Task<bool> DeleteSubmissionAsync(Guid id);
    Task<int> BulkDeleteSubmissionsAsync(Guid formId, IReadOnlyList<Guid> ids);
    Task<FormSubmissionDto> UpdateAdminNotesAsync(Guid id, string? adminNotes);
}
