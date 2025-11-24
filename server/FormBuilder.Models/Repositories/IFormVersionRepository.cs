using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IFormVersionRepository
{
    Task<IEnumerable<FormVersion>> GetVersionsByFormIdAsync(Guid formId);
    Task<FormVersion?> GetVersionAsync(Guid formId, int versionNumber);
    Task<FormVersion?> GetVersionByIdAsync(Guid versionId);
    Task<FormVersion?> GetCurrentVersionAsync(Guid formId);
    Task<FormVersion> CreateAsync(FormVersion version);
    Task<FormVersion?> UpdateAsync(Guid formId, int versionNumber, FormVersion version);
    Task<bool> DeleteAsync(Guid formId, int versionNumber);
}
