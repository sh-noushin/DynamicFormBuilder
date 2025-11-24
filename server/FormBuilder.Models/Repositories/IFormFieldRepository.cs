using FormBuilder.Models.Entities;

namespace FormBuilder.Models.Repositories;

public interface IFormFieldRepository
{
    Task<IEnumerable<FormVersionField>> GetFieldsByVersionIdAsync(Guid versionId);
    Task<FormVersionField?> GetByIdAsync(Guid fieldId);
    Task<FormVersionField> CreateAsync(FormVersionField field);
    Task<FormVersionField?> UpdateAsync(Guid fieldId, FormVersionField field);
    Task<bool> DeleteAsync(Guid fieldId);
    Task<IEnumerable<FormVersionField>> BulkCreateAsync(List<FormVersionField> fields);
    Task<bool> BulkUpdateAsync(List<FormVersionField> fields);
}
